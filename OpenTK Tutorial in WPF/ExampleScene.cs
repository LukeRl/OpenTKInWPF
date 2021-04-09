using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Wpf;

namespace OpenTK_Tutorial_in_WPF
{
    class ExampleScene
    {
        private int VertexBufferObject;
        private Shader shader;
        private int VertexArrayObject;
        private bool Prepared = false;
        private bool Rectangles = false;
        private int ElementBufferObject;
        private readonly float[] RectangleVertices = {
             0.5f,  0.5f, 0.0f,  // top right
             0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f   // top left
            };

        private readonly uint[] RectangleIndices = {  // note that we start from 0!
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

        public void Prepare()
        {
            GL.ClearColor(Color4.Red);

            VertexBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();
            ElementBufferObject = GL.GenBuffer();

            shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");
            shader.Use();

            Matrix4 model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            // Note that we're translating the scene in the reverse direction of where we want to move.
            Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            // TODO - The clipping planes don't appear to be doing anything - why?
            Matrix4 projection = Matrix4.CreateOrthographic(156f / 10, 38f / 10, 0.1f, 100f);

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            Prepared = true;
        }

        public void AddRectangle()
        {
            // Add rectangle vertex data to the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, RectangleVertices.Length * sizeof(float), RectangleVertices, BufferUsageHint.DynamicDraw);

            // Establish the VAO
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Add the order for the rectangles to be drawn in the EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, RectangleIndices.Length * sizeof(uint), RectangleIndices, BufferUsageHint.DynamicDraw);

            Rectangles = true;
        }

        public void Render(double width, double height)
        {
            if (!Prepared)
            {
                return;
            }

            // TODO - add some check for resize - currently just live updating the projection
            // Also updates the size of the canvas - will remove any stretch effects
            Matrix4 projection = Matrix4.CreateOrthographic((float)width / 100, (float)height / 100, 0.1f, 100f);
            shader.SetMatrix4("projection", projection);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            if (Rectangles)
            {
                GL.DrawElements(PrimitiveType.Triangles, RectangleIndices.Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        public void Close()
        {
            // Binding a buffer to 0 basically sets it to null, so any calls that modify a buffer without binding one first will result in a crash. This is easier to debug than accidentally modifying a buffer that we didn't want modified.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Delete all buffer objects
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteBuffer(VertexArrayObject);
            shader.Dispose();
        }
    }
}
