using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenTK_Tutorial_in_WPF
{
    class Texture
    {
        public int Handle;

        public Texture(string path)
        {
            // Generate a blank texture
            Handle = GL.GenTexture();
            // Use the texture
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            // Load the image
            Image<Rgba32> image = Image.Load<Rgba32>(path);

            // ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left,
            // causing the texture to be flipped vertically.
            // This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }

            // Set the image wrap modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // Set the image filtering nodes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Generate the texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
            
            // Optional to generate mipmaps:
            // GL.GenerateMipmaps();
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
