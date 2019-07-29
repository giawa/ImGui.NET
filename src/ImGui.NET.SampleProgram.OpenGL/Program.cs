using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenGL;
using Veldrid.Sdl2;

namespace ImGuiNET
{
    class Program
    {
        private static int _width = 1280, _height = 720;
        private static bool mouseleft, mouseright;
        private static int mouseX, mouseY, mouseWheel;

        static unsafe void Main(string[] args)
        {
            if (Sdl2Native.SDL_Init((SDLInitFlags)62001) < 0)
            {
                Console.WriteLine("SDL failed to init.");
                return;
            }

            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.DoubleBuffer, 1);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.DepthSize, 24);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 3);

            IntPtr window = Sdl2Native.SDL_CreateWindow("Imgui.NET OpenGL Sample Program", 128, 128, _width, _height, SDL_WindowFlags.OpenGL | SDL_WindowFlags.Shown | SDL_WindowFlags.Resizable);

            if (window == IntPtr.Zero)
            {
                Console.WriteLine("SDL could not create a window.");
                return;
            }

            IntPtr context = Sdl2Native.SDL_GL_CreateContext(window);

            if (context == IntPtr.Zero)
            {
                Console.WriteLine("SDL could not create a valid OpenGL context.");
                return;
            }

            Sdl2Native.SDL_GL_SetSwapInterval(1);

            Gui.Init();
            Gui.Reshape(_width, _height);

            SDL_Event sdlEvent;
            bool running = true;

            while (running)
            {
                while (Sdl2Native.SDL_PollEvent(&sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL_EventType.Quit)
                    {
                        running = false;
                    }
                    else if (sdlEvent.type == SDL_EventType.TextInput)
                    {
                        SDL_TextInputEvent textEvent = Unsafe.Read<SDL_TextInputEvent>(&sdlEvent);

                        _keyCharPresses.Clear();
                        var chars = TextInputToChars(textEvent);
                        foreach (var c in chars)
                        {
                            if (!_keyCharPresses.Contains(c)) _keyCharPresses.Add(c);
                        }
                    }
                    else if (sdlEvent.type == SDL_EventType.KeyDown)
                    {
                        SDL_KeyboardEvent keyEvent = Unsafe.Read<SDL_KeyboardEvent>(&sdlEvent);

                        switch (keyEvent.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_LCTRL:
                                _lcontrolDown = true;
                                break;
                            case SDL_Keycode.SDLK_RCTRL:
                                _rcontrolDown = true;
                                break;
                            case SDL_Keycode.SDLK_LSHIFT:
                                _lshiftDown = true;
                                break;
                            case SDL_Keycode.SDLK_RSHIFT:
                                _rshiftDown = true;
                                break;
                            case SDL_Keycode.SDLK_LALT:
                                _laltDown = true;
                                break;
                            case SDL_Keycode.SDLK_RALT:
                                _raltDown = true;
                                break;
                            case SDL_Keycode.SDLK_APPLICATION:
                                _winKeyDown = true;
                                break;
                            default:
                                var c = KeycodeToChar(keyEvent.keysym.sym);
                                if (c != ' ') _keysDown.Add(c);
                                break;
                        }
                    }
                    else if (sdlEvent.type == SDL_EventType.KeyUp)
                    {
                        SDL_KeyboardEvent keyEvent = Unsafe.Read<SDL_KeyboardEvent>(&sdlEvent);

                        switch (keyEvent.keysym.sym)
                        {
                            case SDL_Keycode.SDLK_LCTRL:
                                _lcontrolDown = false;
                                break;
                            case SDL_Keycode.SDLK_RCTRL:
                                _rcontrolDown = false;
                                break;
                            case SDL_Keycode.SDLK_LSHIFT:
                                _lshiftDown = false;
                                break;
                            case SDL_Keycode.SDLK_RSHIFT:
                                _rshiftDown = false;
                                break;
                            case SDL_Keycode.SDLK_LALT:
                                _laltDown = false;
                                break;
                            case SDL_Keycode.SDLK_RALT:
                                _raltDown = false;
                                break;
                            case SDL_Keycode.SDLK_APPLICATION:
                                _winKeyDown = false;
                                break;
                            default:
                                var c = KeycodeToChar(keyEvent.keysym.sym);
                                if (c != ' ') _keysUp.Add(c);
                                break;
                        }
                    }
                    else if (sdlEvent.type == SDL_EventType.MouseButtonDown)
                    {
                        SDL_MouseButtonEvent buttonEvent = Unsafe.Read<SDL_MouseButtonEvent>(&sdlEvent);

                        if (buttonEvent.button == SDL_MouseButton.Left) mouseleft = true;
                        if (buttonEvent.button == SDL_MouseButton.Right) mouseright = true;
                    }
                    else if (sdlEvent.type == SDL_EventType.MouseButtonUp)
                    {
                        SDL_MouseButtonEvent buttonEvent = Unsafe.Read<SDL_MouseButtonEvent>(&sdlEvent);

                        if (buttonEvent.button == SDL_MouseButton.Left) mouseleft = false;
                        if (buttonEvent.button == SDL_MouseButton.Right) mouseright = false;
                    }
                    else if (sdlEvent.type == SDL_EventType.MouseMotion)
                    {
                        SDL_MouseMotionEvent motionEvent = Unsafe.Read<SDL_MouseMotionEvent>(&sdlEvent);

                        mouseX = motionEvent.x;
                        mouseY = motionEvent.y;
                    }
                    else if (sdlEvent.type == SDL_EventType.MouseWheel)
                    {
                        SDL_MouseWheelEvent wheelEvent = Unsafe.Read<SDL_MouseWheelEvent>(&sdlEvent);

                        mouseWheel = wheelEvent.y;
                    }
                    else if (sdlEvent.type == SDL_EventType.WindowEvent)
                    {
                        SDL_WindowEvent windowEvent = Unsafe.Read<SDL_WindowEvent>(&sdlEvent);

                        switch (windowEvent.@event)
                        {
                            case SDL_WindowEventID.Resized:
                                OnReshape(windowEvent.data1, windowEvent.data2);
                                break;
                        }
                    }
                }

                OnRenderFrame();
                Sdl2Native.SDL_GL_SwapWindow(window);
            }

            Gui.Dispose();
            Sdl2Native.SDL_GL_DeleteContext(context);
            Sdl2Native.SDL_DestroyWindow(window);
        }

        private static char KeycodeToChar(SDL_Keycode keycode)
        {
            switch (keycode)
            {
                case SDL_Keycode.SDLK_TAB:
                case SDL_Keycode.SDLK_DELETE:
                case SDL_Keycode.SDLK_BACKSPACE:
                case SDL_Keycode.SDLK_ESCAPE:
                    return (char)keycode;
                case SDL_Keycode.SDLK_UP:
                    return (char)45;
                case SDL_Keycode.SDLK_DOWN:
                    return (char)46;
                case SDL_Keycode.SDLK_LEFT:
                    return (char)47;
                case SDL_Keycode.SDLK_RIGHT:
                    return (char)48;
                case SDL_Keycode.SDLK_PAGEUP:
                    return (char)56;
                case SDL_Keycode.SDLK_PAGEDOWN:
                    return (char)57;
                case SDL_Keycode.SDLK_HOME:
                    return (char)58;
                case SDL_Keycode.SDLK_END:
                    return (char)59;
                case SDL_Keycode.SDLK_RETURN:
                    return (char)49;
            }

            return ' ';
        }

        private static readonly char[] _emptyCharArray = new char[0];

        private unsafe static char[] TextInputToChars(SDL_TextInputEvent textInputEvent)
        {
            int byteCount = 0;
            // Loop until the null terminator is found or the max size is reached.
            while (byteCount < SDL_TextInputEvent.MaxTextSize && textInputEvent.text[byteCount++] != 0)
            { }

            if (byteCount > 1)
            {
                // We don't want the null terminator.
                byteCount -= 1;
                byte[] bytesArray = new byte[byteCount];
                Marshal.Copy((IntPtr)textInputEvent.text, bytesArray, 0, byteCount);
                int charCount = Encoding.UTF8.GetCharCount(textInputEvent.text, (int)byteCount);
                char[] charsPtr = new char[charCount];
                Encoding.UTF8.GetChars(bytesArray, 0, byteCount, charsPtr, 0);
                return charsPtr;
            }
            else return _emptyCharArray;
        }

        private static void OnRenderFrame()
        {
            Gl.Viewport(0, 0, _width, _height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UpdateImGuiInput();

            ImGui.NewFrame();
            ImGui.ShowDemoWindow();

            ImGui.Render();

            var drawData = ImGui.GetDrawData();
            Gui.RenderImDrawData(drawData);
        }

        private static void OnReshape(int width, int height)
        {
            _width = width;
            _height = height;
            Gui.Reshape(_width, _height);
        }

        private static bool _lcontrolDown, _lshiftDown, _laltDown, _rcontrolDown, _rshiftDown, _raltDown, _winKeyDown;
        private static HashSet<char> _keyCharPresses = new HashSet<char>();
        private static HashSet<char> _keysDown = new HashSet<char>();
        private static HashSet<char> _keysUp = new HashSet<char>();

        private static void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            Vector2 mousePosition = new Vector2(mouseX, mouseY);

            io.MouseDown[0] = mouseleft;
            io.MouseDown[1] = mouseright;
            io.MouseDown[2] = false;
            io.MousePos = mousePosition;
            io.MouseWheel = mouseWheel;

            mouseWheel = 0;

            foreach (var key in _keyCharPresses)
                io.AddInputCharacter(key);
            _keyCharPresses.Clear();

            foreach (var key in _keysDown)
                io.KeysDown[key] = true;
            foreach (var key in _keysUp)
                io.KeysDown[key] = false;

            _keysDown.Clear();
            _keysUp.Clear();

            io.KeyCtrl = _lcontrolDown | _rcontrolDown;
            io.KeyAlt = _laltDown | _raltDown;
            io.KeyShift = _lshiftDown | _rshiftDown;
            io.KeySuper = _winKeyDown;
        }
    }
}
