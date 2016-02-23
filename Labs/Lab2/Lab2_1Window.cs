using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab2
{
    class Lab2_1Window : GameWindow
    {        
        private int[] mTriangleVertexBufferObjectIDArray = new int[2];
        private int[] mSquareVertexBufferObjectIDArray = new int[2];
        private int[] mVertexArrayObjectIDs = new int[2];
        private ShaderUtility mShader;

        public Lab2_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_1 Linking to Shaders and VAOs",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.CadetBlue);
            GL.Enable(EnableCap.DepthTest);

            #region squareVertices

            float[] squareVertices = new float[] { -0.2f, -0.4f, 0.2f, 1.0f, 1.0f, 0.0f,
                                                   0.8f, -0.4f, 0.2f, 1.0f, 0.0f, 1.0f,
                                                   0.8f, 0.6f, 0.2f, 1.0f, 1.0f, 0.0f,
                                                   -0.2f, 0.6f, 0.2f, 1.0f, 0.0f, 1.0f};
            
            #endregion

            #region triangleVertices

            float[] triangleVertices = new float[] { -0.8f, 0.8f, 0.4f, 1.0f, 0.0f, 0.0f,
                                                     -0.6f, -0.4f, 0.4f, 0.0f, 1.0f, 0.0f,
                                                     0.2f, 0.2f, 0.4f, 0.0f, 0.0f, 1.0f};
            
            #endregion

            #region squareIndices

            uint[] squareIndices = new uint[] { 0, 1, 2, 3 };
            
            #endregion

            #region triangleIndices

            uint[] triangleIndices = new uint[] { 0, 1, 2 };
            
            #endregion

            #region squareVertices BindBuffer

            GL.GenBuffers(2, mSquareVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(squareVertices.Length * sizeof(float)), squareVertices, BufferUsageHint.StaticDraw);

            int squareSize;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out squareSize);

            if (squareVertices.Length * sizeof(float) != squareSize)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            
            #endregion

            #region triangleVertices BindBuffer

            GL.GenBuffers(2, mTriangleVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mTriangleVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(triangleVertices.Length * sizeof(float)), triangleVertices, BufferUsageHint.StaticDraw);

            int triangleSize;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out triangleSize);

            if (triangleVertices.Length * sizeof(float) != triangleSize)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            
            #endregion

            #region squareIndices BindBuffer

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mTriangleVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(triangleIndices.Length * sizeof(int)), triangleIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out triangleSize);

            if (triangleIndices.Length * sizeof(int) != triangleSize)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }
            
            
            #endregion

            #region triangleIndices BindBuffer

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(squareIndices.Length * sizeof(int)), squareIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out squareSize);

            if (squareIndices.Length * sizeof(int) != squareSize)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }
            
            #endregion

            #region Shader Loading Code

            mShader = new ShaderUtility(@"Lab2/Shaders/vLab21.vert", @"Lab2/Shaders/fSimple.frag");

            GL.GenVertexArrays(2, mVertexArrayObjectIDs);

            #region Square Shader Loading Code

            GL.BindVertexArray(mVertexArrayObjectIDs[0]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareVertexBufferObjectIDArray[1]);

            GL.UseProgram(mShader.ShaderProgramID);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region Triangle Shader Loading Code

            GL.BindVertexArray(mVertexArrayObjectIDs[1]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mTriangleVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mTriangleVertexBufferObjectIDArray[1]);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            #region Square Drawing Code

            GL.BindVertexArray(mVertexArrayObjectIDs[0]);

            GL.DrawElements(PrimitiveType.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region Triangle Drawing Code

            GL.BindVertexArray(mVertexArrayObjectIDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);

            #endregion

            GL.BindVertexArray(0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            GL.DeleteBuffers(2, mSquareVertexBufferObjectIDArray);
            GL.DeleteBuffers(2, mTriangleVertexBufferObjectIDArray);

            GL.BindVertexArray(0);
            GL.DeleteVertexArrays(2, mVertexArrayObjectIDs);

            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
