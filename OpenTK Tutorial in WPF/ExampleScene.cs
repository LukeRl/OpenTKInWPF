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
        private double CurrentWidth;
        private double CurrentHeight;

        private int VertexBufferObject;
        private Shader shader;
        private int VertexArrayObject;
        private bool Prepared = false;
        private int SelectedRectangle = -1;
        private int ElementBufferObject;
        private List<Matrix4> RectangleTransforms;
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
            Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -0.3f);
            Matrix4 projection = Matrix4.CreateOrthographic(156f / 10, 38f / 10, -0.1f, 100f);

            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);

            RectangleTransforms = new List<Matrix4>();

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

            // Add the transform for this model
            Matrix4 model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            RectangleTransforms.Add(model);
        }

        public void Render(double width, double height)
        {
            if (!Prepared)
            {
                return;
            }

            CurrentWidth = width;
            CurrentHeight = height;

            // TODO - add some check for resize - currently just live updating the projection
            // Also updates the size of the canvas - will remove any stretch effects
            Matrix4 projection = Matrix4.CreateOrthographic((float)width / 100, (float)height / 100, 0.1f, 100f);
            //Matrix4 projection = Matrix4.CreateOrthographic((float)width / 100, (float)height / 100, 0.1f, 100f);
            shader.SetMatrix4("projection", projection);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(VertexArrayObject);

            for (int i = 0; i < RectangleTransforms.Count; i++)
            {
                shader.SetMatrix4("model", RectangleTransforms[i]);
                GL.DrawElements(PrimitiveType.Triangles, RectangleIndices.Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        public void ProcessMouseDown(double x, double y)
        {
            // Convert click coordinates to canvas coordinates
            double canvasX = (x / 100) - (CurrentWidth / 200);
            double canvasY = -((y / 100) - (CurrentHeight / 200));

            // Find the intersections
            for (int i = 0; i < RectangleTransforms.Count; i++)
            {
                Vector4 movedTopRight = new Vector4(RectangleVertices[0], RectangleVertices[1], RectangleVertices[2], 1) * RectangleTransforms[i];
                Vector4 movedBottomLeft = new Vector4(RectangleVertices[6], RectangleVertices[7], RectangleVertices[8], 1) * RectangleTransforms[i];
                if (canvasX >= movedBottomLeft.X && canvasX <= movedTopRight.X && canvasY <= movedTopRight.Y && canvasY >= movedBottomLeft.Y)
                {
                    // Move the rectangle
                    SelectedRectangle = i;
                }
            }
        }

        public void ProcessMouseDrag(double x, double y)
        {
            if (SelectedRectangle == -1)
            {
                return;
            }
            double canvasX = (x / 100) - (CurrentWidth / 200);
            double canvasY = -((y / 100) - (CurrentHeight / 200));
            // Move the rectangle
            RectangleTransforms[SelectedRectangle] = Matrix4.CreateTranslation((float)canvasX, (float)canvasY, 0.0f);
        }

        public void ProcessMouseUp()
        {
            SelectedRectangle = -1;
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
