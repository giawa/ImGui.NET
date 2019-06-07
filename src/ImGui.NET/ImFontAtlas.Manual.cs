using System;
using System.Runtime.InteropServices;

namespace ImGuiNET
{
    public unsafe partial struct ImFontAtlasPtr
    {
        public void GetTexDataAsAlpha8(
            out byte[] out_pixels, 
            out int out_width, 
            out int out_height)
        {
            int out_bytes_per_pixel;
            byte* pixelPtr = null;
            GetTexDataAsAlpha8(out pixelPtr, out out_width, out out_height, out out_bytes_per_pixel);
            out_pixels = new byte[out_width * out_height * out_bytes_per_pixel];
            Marshal.Copy((IntPtr)pixelPtr, out_pixels, 0, out_pixels.Length);
        }

        public void GetTexDataAsAlpha8(
            out byte[] out_pixels, 
            out int out_width, 
            out int out_height, 
            out int out_bytes_per_pixel)
        {
            byte* pixelPtr = null;
            GetTexDataAsAlpha8(out pixelPtr, out out_width, out out_height, out out_bytes_per_pixel);
            out_pixels = new byte[out_width * out_height * out_bytes_per_pixel];
            Marshal.Copy((IntPtr)pixelPtr, out_pixels, 0, out_pixels.Length);
        }

        public void GetTexDataAsRGBA32(
            out byte[] out_pixels, 
            out int out_width, 
            out int out_height)
        {
            int out_bytes_per_pixel;
            byte* pixelPtr = null;
            GetTexDataAsRGBA32(out pixelPtr, out out_width, out out_height, out out_bytes_per_pixel);
            out_pixels = new byte[out_width * out_height * out_bytes_per_pixel];
            Marshal.Copy((IntPtr)pixelPtr, out_pixels, 0, out_pixels.Length);
        }

        public void GetTexDataAsRGBA32(
            out byte[] out_pixels, 
            out int out_width, 
            out int out_height, 
            out int out_bytes_per_pixel)
        {
            byte* pixelPtr = null;
            GetTexDataAsRGBA32(out pixelPtr, out out_width, out out_height, out out_bytes_per_pixel);
            out_pixels = new byte[out_width * out_height * out_bytes_per_pixel];
            Marshal.Copy((IntPtr)pixelPtr, out_pixels, 0, out_pixels.Length);
        }
    }
}
