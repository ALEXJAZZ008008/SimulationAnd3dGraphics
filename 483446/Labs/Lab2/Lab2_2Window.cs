using OpenTK;
using System;
using OpenTK.Graphics;
using Labs.Utility;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab2
{
    public class Lab2_2Window : GameWindow
    {
        public Lab2_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_2 Understanding the Camera",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[2];
        private int mVAO_ID;
        private ShaderUtility mShader;
        private ModelUtility mModel;
        private Matrix4 mView;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.01f;

            if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateRotationX(-cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateRotationY(-cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateRotationX(cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateRotationY(cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 'q')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, cameraSpeed * 10);
                MoveCamera();
            }

            if (e.KeyChar == 'e')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0, -cameraSpeed * 10);
                MoveCamera();
            }

            if (e.KeyChar == 'z')
            {
                mView = mView * Matrix4.CreateRotationZ(cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 'c')
            {
                mView = mView * Matrix4.CreateRotationZ(-cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 't')
            {
                mView = mView * Matrix4.CreateTranslation(0, -cameraSpeed, 0);
                MoveCamera();
            }

            if (e.KeyChar == 'f')
            {
                mView = mView * Matrix4.CreateTranslation(cameraSpeed, 0, 0);
                MoveCamera();
            }

            if (e.KeyChar == 'g')
            {
                mView = mView * Matrix4.CreateTranslation(0, cameraSpeed, 0);
                MoveCamera();
            }

            if (e.KeyChar == 'h')
            {
                mView = mView * Matrix4.CreateTranslation(-cameraSpeed, 0, 0);
                MoveCamera();
            }
        }

        private void MoveCamera()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);

            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");    
            mShader = new ShaderUtility(@"Lab2/Shaders/vLab22.vert", @"Lab2/Shaders/fSimple.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            mVAO_ID = GL.GenVertexArray();
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            
            GL.BindVertexArray(mVAO_ID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModel.Indices.Length * sizeof(float)), mModel.Indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, 0, -2);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            base.OnLoad(e);
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(mVAO_ID);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            Matrix4 m1 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m2 = Matrix4.CreateTranslation(0, 0, 0);
            Matrix4 m3 = m1 * m2;
            GL.UniformMatrix4(uModelLocation, true, ref m3);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m4 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m5 = Matrix4.CreateTranslation(0.3f, 0.3f, 0);
            Matrix4 m6 = m4 * m5;
            GL.UniformMatrix4(uModelLocation, true, ref m6);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m7 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m8 = Matrix4.CreateTranslation(-0.3f, -0.3f, 0);
            Matrix4 m9 = m7 * m8;
            GL.UniformMatrix4(uModelLocation, true, ref m9);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m10 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m11 = Matrix4.CreateTranslation(0.3f, -0.3f, 0);
            Matrix4 m12 = m10 * m11;
            GL.UniformMatrix4(uModelLocation, true, ref m12);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m13 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m14 = Matrix4.CreateTranslation(-0.3f, 0.3f, 0);
            Matrix4 m15 = m13 * m14;
            GL.UniformMatrix4(uModelLocation, true, ref m15);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m16 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m17 = Matrix4.CreateTranslation(0.6f, 0, 0);
            Matrix4 m18 = m16 * m17;
            GL.UniformMatrix4(uModelLocation, true, ref m18);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m19 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m20 = Matrix4.CreateTranslation(-0.6f, 0, 0);
            Matrix4 m21 = m19 * m20;
            GL.UniformMatrix4(uModelLocation, true, ref m21);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m22 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m23 = Matrix4.CreateTranslation(0, 0.6f, 0);
            Matrix4 m24 = m22 * m23;
            GL.UniformMatrix4(uModelLocation, true, ref m24);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m25 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m26 = Matrix4.CreateTranslation(0, -0.6f, 0);
            Matrix4 m27 = m25 * m26;
            GL.UniformMatrix4(uModelLocation, true, ref m27);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m28 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m29 = Matrix4.CreateTranslation(0, 0, 0.5f);
            Matrix4 m30 = m28 * m29;
            GL.UniformMatrix4(uModelLocation, true, ref m30);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m31 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m32 = Matrix4.CreateTranslation(0.3f, 0.3f, 0.5f);
            Matrix4 m33 = m31 * m32;
            GL.UniformMatrix4(uModelLocation, true, ref m33);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m34 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m35 = Matrix4.CreateTranslation(-0.3f, -0.3f, 0.5f);
            Matrix4 m36 = m34 * m35;
            GL.UniformMatrix4(uModelLocation, true, ref m36);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m37 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m38 = Matrix4.CreateTranslation(0.3f, -0.3f, 0.5f);
            Matrix4 m39 = m37 * m38;
            GL.UniformMatrix4(uModelLocation, true, ref m39);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m40 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m41 = Matrix4.CreateTranslation(-0.3f, 0.3f, 0.5f);
            Matrix4 m42 = m40 * m41;
            GL.UniformMatrix4(uModelLocation, true, ref m42);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m43 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m44 = Matrix4.CreateTranslation(0.6f, 0, 0.5f);
            Matrix4 m45 = m43 * m44;
            GL.UniformMatrix4(uModelLocation, true, ref m45);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m46 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m47 = Matrix4.CreateTranslation(-0.6f, 0, 0.5f);
            Matrix4 m48 = m46 * m47;
            GL.UniformMatrix4(uModelLocation, true, ref m48);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m49 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m50 = Matrix4.CreateTranslation(0, 0.6f, 0.5f);
            Matrix4 m51 = m49 * m50;
            GL.UniformMatrix4(uModelLocation, true, ref m51);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m52 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m53 = Matrix4.CreateTranslation(0, -0.6f, 0.5f);
            Matrix4 m54 = m52 * m53;
            GL.UniformMatrix4(uModelLocation, true, ref m54);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m55 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m56 = Matrix4.CreateTranslation(0, 0, -0.5f);
            Matrix4 m57 = m55 * m56;
            GL.UniformMatrix4(uModelLocation, true, ref m57);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m58 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m59 = Matrix4.CreateTranslation(0.3f, 0.3f, -0.5f);
            Matrix4 m60 = m58 * m59;
            GL.UniformMatrix4(uModelLocation, true, ref m60);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m61 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m62 = Matrix4.CreateTranslation(-0.3f, -0.3f, -0.5f);
            Matrix4 m63 = m61 * m62;
            GL.UniformMatrix4(uModelLocation, true, ref m63);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m64 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m65 = Matrix4.CreateTranslation(0.3f, -0.3f, -0.5f);
            Matrix4 m66 = m64 * m65;
            GL.UniformMatrix4(uModelLocation, true, ref m66);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m67 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m68 = Matrix4.CreateTranslation(-0.3f, 0.3f, -0.5f);
            Matrix4 m69 = m67 * m68;
            GL.UniformMatrix4(uModelLocation, true, ref m69);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m70 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m71 = Matrix4.CreateTranslation(0.6f, 0, -0.5f);
            Matrix4 m72 = m70 * m71;
            GL.UniformMatrix4(uModelLocation, true, ref m72);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m73 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m74 = Matrix4.CreateTranslation(-0.6f, 0, -0.5f);
            Matrix4 m75 = m73 * m74;
            GL.UniformMatrix4(uModelLocation, true, ref m75);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m76 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m77 = Matrix4.CreateTranslation(0, 0.6f, -0.5f);
            Matrix4 m78 = m76 * m77;
            GL.UniformMatrix4(uModelLocation, true, ref m78);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m79 = Matrix4.CreateRotationZ(0.8f);
            Matrix4 m80 = Matrix4.CreateTranslation(0, -0.6f, -0.5f);
            Matrix4 m81 = m79 * m80;
            GL.UniformMatrix4(uModelLocation, true, ref m81);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArray(mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
