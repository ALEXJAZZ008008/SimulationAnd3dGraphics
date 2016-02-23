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

            #region squareVertices

            float[] squareVertices = new float[] { -0.2f, -0.4f, 0.2f,
                                                   0.8f, -0.4f, 0.2f,
                                                   0.8f, 0.6f, 0.2f,
                                                   -0.2f, 0.6f, 0.2f};
            
            #endregion

            #region triangleVertices

            float[] triangleVertices = new float[] { -0.8f, 0.8f, 0.4f,
                                                     -0.6f, -0.4f, 0.4f,
                                                     0.2f, 0.2f, 0.4f};
            
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

            #endregion

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            #region Square Drawing Code

            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareVertexBufferObjectIDArray[1]);
            
            #region Shader Loading Code
            
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 3 *sizeof(float), 0);

            #endregion

            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");
            GL.Uniform4(uColourLocation, Color4.Blue);

            GL.DrawElements(PrimitiveType.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);
            
            #endregion

            #region Triangle Drawing Code

            GL.Uniform4(uColourLocation, Color4.Red);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mTriangleVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mTriangleVertexBufferObjectIDArray[1]);

            #region Shader Loading Code

            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 3 *sizeof(float), 0);

            #endregion

            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);
            
            #endregion

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(2, mTriangleVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
