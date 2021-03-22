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
    // Not sure if GLWpfController handles this, but we are missing swapbuffers, onresize and onunload
    // Don't need double buffers - I think this is auto done through directX interop
    class ExampleScene
    {
        private int VertexBufferObject;
        private Shader shader;
        private int VertexArrayObject;
        private bool prepared = false;
        // Rectangle
        private int ElementBufferObject;
        private float[] RectangleVertices = {
             0.5f,  0.5f, 0.0f,  // top right
             0.5f, -0.5f, 0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f   // top left
            };

        private uint[] RectangleIndices = {  // note that we start from 0!
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };
        private Stopwatch timer;

        private Texture texture;

        public void Prepare()
        {
            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, RectangleVertices.Length * sizeof(float), RectangleVertices, BufferUsageHint.StaticDraw);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // It's almost exactly the same as how you use the VBO! Most of OpenGL's buffer types will follow this pattern:
            // Create with GL.GenBuffer(), bind with GL.BindBuffer, and then use GL.BufferData to add data to it.
            // We create/bind the Element Buffer Object EBO the same way as the VBO, except there is a major difference here which can be REALLY confusing.
            // The binding spot for ElementArrayBuffer is not actually a global binding spot like ArrayBuffer is. 
            // Instead it's actually a property of the currently bound VertexArrayObject, and binding an EBO with no VAO is undefined behaviour.
            // This also means that if you bind another VAO, the current ElementArrayBuffer is going to change with it.
            // Another sneaky part is that you don't need to unbind the buffer in ElementArrayBuffer as unbinding the VAO is going to do this,
            // and unbinding the EBO will remove it from the VAO instead of unbinding it like you would for VBOs or VAOs.
            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            // We also upload data to the EBO the same way as we did with VBOs.
            GL.BufferData(BufferTarget.ElementArrayBuffer, RectangleIndices.Length * sizeof(uint), RectangleIndices, BufferUsageHint.StaticDraw);
            // The EBO has now been properly setup. Go to the Render function to see how we draw our rectangle now!


            timer = new Stopwatch();
            timer.Start();
            shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");
            prepared = true;
        }
        public void PrepareTriangle()
        {
            VertexBufferObject = GL.GenBuffer();
            VertexArrayObject = GL.GenVertexArray();
            // This locks any buffer calls we make to the currently bound buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            // Triangle of vertices
            float[] vertices =
            {
                //Position          Texture coordinates
                 -0.5f,  -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                 0.0f, 0.5f, 0.0f, 0.5f, 1.0f, // top
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            };

            shader = new Shader("../../../Shaders/shader.vert", "../../../Shaders/shader.frag");
            shader.Use();

            /*
            // Add the vertices to the buffer's memory
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                vertices, BufferUsageHint.StaticDraw);
            // StaticDraw: the data will most likely not change at all or very rarely.
            // DynamicDraw: the data is likely to change a lot.
            // StreamDraw: the data will change every time it is drawn.

            // Tell openGL how to interpret the vertex data
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            */


            // Handling a VAO
            // ..:: Initialization code (done once (unless your object frequently changes)) ::..
            // 1. bind Vertex Array Object
            GL.BindVertexArray(VertexArrayObject);
            // 2. copy our vertices array in a buffer for OpenGL to use
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            // 3. then set our vertex attributes pointers
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            //GL.EnableVertexAttribArray(0);
            //GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            //GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            texture = new Texture("../../../Resources/container.png");
            texture.Use(TextureUnit.Texture0);

            // Set up some transformations
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90.0f));
            Matrix4 scale = Matrix4.CreateScale(0.5f, 1f, 0.5f);
            Matrix4 transform = rotation * scale;

            shader.SetMatrix4("transform", transform);

            prepared = true;
        }

        public void Render()
        {
            if (!prepared)
            {
                return;
            }

            GL.ClearColor(Color4.Red); // Can set this in the prepare rather than each render call
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            // double timeValue = timer.Elapsed.TotalSeconds;
            // float greenValue = (float)Math.Sin(timeValue) / (2.0f + 0.5f);
           //  int vertexColorLocation = GL.GetUniformLocation(shader.Handle, "vertexColor");
            // GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            // GL.DrawElements(PrimitiveType.Triangles, RectangleIndices.Length, DrawElementsType.UnsignedInt, 0);
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
