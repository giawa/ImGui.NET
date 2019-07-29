using OpenGL;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace ImGuiNET
{
    public static class Gui
    {
        private static ShaderProgram _shaderProgram;

        private static int _texAttribLocation = 0, _projMatrixAttribLocation = 0;
        private static int _vertPosAttribLocation = 0, _vertUVAttribLocation = 0, _vertColorAttribLocation = 0;

        private static Texture _fontTexture;

        private static uint _vboBuffer, _elementBuffer;

        private static int _width, _height;

        public static void Reshape(int width, int height)
        {
            _width = width;
            _height = height;
        }

        private static string LoadEmbeddedShaderCode(string name)
        {
            string resourceName = name + ".glsl";
            byte[] data = GetEmbeddedResourceBytes(resourceName);
            return Encoding.UTF8.GetString(data);
        }

        private static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            Assembly assembly = typeof(Gui).Assembly;
            var temp = assembly.GetManifestResourceNames();
            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ret = new byte[s.Length];
                s.Read(ret, 0, (int)s.Length);
                return ret;
            }
        }

        public static void Init()
        {
            // compile the shader program
            string vertexShader = LoadEmbeddedShaderCode("imgui-vertex");
            string fragmentShader = LoadEmbeddedShaderCode("imgui-frag");
            _shaderProgram = new ShaderProgram(vertexShader, fragmentShader);

            _shaderProgram.Use();
            _texAttribLocation = _shaderProgram["FontTexture"].Location;
            _projMatrixAttribLocation = _shaderProgram["projection_matrix"].Location;
            _vertPosAttribLocation = Gl.GetAttribLocation(_shaderProgram.ProgramID, "in_position");
            _vertUVAttribLocation = Gl.GetAttribLocation(_shaderProgram.ProgramID, "in_texCoord");
            _vertColorAttribLocation = Gl.GetAttribLocation(_shaderProgram.ProgramID, "in_color");

            _vboBuffer = Gl.GenBuffer();
            _elementBuffer = Gl.GenBuffer();

            var imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imguiContext);
            ImGui.GetIO().Fonts.AddFontDefault();
            RecreateFontDeviceTexture();
            ImGui.StyleColorsDark();

            SetKeyMappings();
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_TAB;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = 47;
            io.KeyMap[(int)ImGuiKey.RightArrow] = 48;
            io.KeyMap[(int)ImGuiKey.UpArrow] = 45;
            io.KeyMap[(int)ImGuiKey.DownArrow] = 46;
            io.KeyMap[(int)ImGuiKey.PageUp] = 56;
            io.KeyMap[(int)ImGuiKey.PageDown] = 57;
            io.KeyMap[(int)ImGuiKey.Home] = 58;
            io.KeyMap[(int)ImGuiKey.End] = 59;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_DELETE;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_BACKSPACE;
            io.KeyMap[(int)ImGuiKey.Enter] = 49;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_ESCAPE;
            io.KeyMap[(int)ImGuiKey.A] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_a;
            io.KeyMap[(int)ImGuiKey.C] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_c;
            io.KeyMap[(int)ImGuiKey.V] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_v;
            io.KeyMap[(int)ImGuiKey.X] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_x;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Veldrid.Sdl2.SDL_Keycode.SDLK_z;
        }

        public static void Dispose()
        {
            // dispose of all of the resources that were created
            _fontTexture.Dispose();
            _shaderProgram.DisposeChildren = true;
            _shaderProgram.Dispose();
        }

        public static void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            // get the font texture from imgui
            IntPtr pixels;
            int width, height, bytesPerPixel;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);
            io.Fonts.SetTexID((IntPtr)1);

            // take the bytes returned by the font texture and then turn it into a texture
            _fontTexture = new Texture(pixels, width, height, PixelFormat.Rgba, PixelInternalFormat.Rgba);

            io.Fonts.ClearTexData();
        }

        public static void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            // really these properties should be read and then reset back to what they were
            Gl.Enable(EnableCap.Blend);
            Gl.BlendEquation(BlendEquationMode.FuncAdd);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            Gl.Disable(EnableCap.CullFace);
            Gl.Disable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.ScissorTest);

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(_width, _height);

            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);

            _shaderProgram.Use();
            Gl.Uniform1f(_texAttribLocation, 0);
            Gl.UniformMatrix4fv(_projMatrixAttribLocation, mvp);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vboBuffer);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBuffer);

            Gl.EnableVertexAttribArray(_vertPosAttribLocation);
            Gl.EnableVertexAttribArray(_vertUVAttribLocation);
            Gl.EnableVertexAttribArray(_vertColorAttribLocation);
            Gl.VertexAttribPointer(_vertPosAttribLocation, 2, VertexAttribPointerType.Float, false, 20, (IntPtr)0);
            Gl.VertexAttribPointer(_vertUVAttribLocation, 2, VertexAttribPointerType.Float, false, 20, (IntPtr)8);
            Gl.VertexAttribPointer(_vertColorAttribLocation, 4, VertexAttribPointerType.UnsignedByte, true, 20, (IntPtr)16);

            var clip_off = draw_data.DisplayPos;         // (0,0) unless using multi-viewports
            var clip_scale = draw_data.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                Gl.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cmd_list.VtxBuffer.Size * 20), cmd_list.VtxBuffer.Data, BufferUsageHint.DynamicDraw);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(cmd_list.IdxBuffer.Size * 2), cmd_list.IdxBuffer.Data, BufferUsageHint.DynamicDraw);

                int idx_offset = 0;
                int vtx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    var pcmd = cmd_list.CmdBuffer[cmd_i];

                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        Console.WriteLine("Unknown");
                    }
                    else
                    {
                        Vector4 clip_rect;
                        clip_rect.X = (pcmd.ClipRect.X - clip_off.X) * clip_scale.X;
                        clip_rect.Y = (pcmd.ClipRect.Y - clip_off.Y) * clip_scale.Y;
                        clip_rect.Z = (pcmd.ClipRect.Z - clip_off.X) * clip_scale.X;
                        clip_rect.W = (pcmd.ClipRect.W - clip_off.Y) * clip_scale.Y;

                        if (clip_rect.X < _width && clip_rect.Y < _height && clip_rect.Z >= 0.0f && clip_rect.W >= 0.0f)
                        {
                            // apply scissor/clipping rectangle
                            Gl.Scissor((int)clip_rect.X, (int)(_height - clip_rect.W), (int)(clip_rect.Z - clip_rect.X), (int)(clip_rect.W - clip_rect.Y));

                            // set the requested texture
                            Gl.BindTexture(TextureTarget.Texture2D, (uint)pcmd.TextureId);

                            Gl.DrawElementsBaseVertex(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * 2), vtx_offset);
                        }
                        else
                        {
                            Console.WriteLine("Missed test");
                        }

                        idx_offset += (int)pcmd.ElemCount;
                    }
                }

                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            Gl.Disable(EnableCap.ScissorTest);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.Disable(EnableCap.Blend);
        }
    }
}
