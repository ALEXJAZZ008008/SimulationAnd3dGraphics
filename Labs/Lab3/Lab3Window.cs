using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab3
{
    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {

        }

        private int[] mVBO_IDs = new int[5];
        private int[] mVAO_IDs = new int[3];
        private ShaderUtility mShader;
        private ModelUtility mModelModelUtility, mCylinderModelUtility;
        private Matrix4 mView, mModelModel, mCylinderModel, mGroundModel;

        protected override void OnLoad(EventArgs e)
        {
            int size;

            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mShader = new ShaderUtility(@"Lab3/Shaders/vPassThrough.vert", @"Lab3/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID,"uLight.Position");
            int uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID,"uLight.AmbientLight");
            int uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.DiffuseLight");
            int uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.SpecularLight");

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] { -10, 0, -10, 0, 1, 0,
                                             -10, 0, 10, 0, 1, 0,
                                             10, 0, 10, 0, 1, 0,
                                             10, 0, -10, 0, 1, 0 };

            GL.BindVertexArray(mVAO_IDs[0]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            mModelModelUtility = ModelUtility.LoadModel(@"Utility/Models/model1.bin"); 

            GL.BindVertexArray(mVAO_IDs[1]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModelModelUtility.Vertices.Length * sizeof(float)), mModelModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModelModelUtility.Indices.Length * sizeof(float)), mModelModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            
            if (mModelModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            
            if (mModelModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[2]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            
            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            GL.UniformMatrix4(uView, true, ref mView);

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mModelModel = Matrix4.CreateTranslation(0, 1, -5f);
            mCylinderModel = Matrix4.CreateTranslation(0, 0, -5f);

            Vector4 eyePosition = Vector4.Transform(new Vector4(0, 0, 0, 1), mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);

            Vector4 lightPosition = Vector4.Transform(new Vector4(2, 4, -8.5f, 1), mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            Vector3 colour = new Vector3(1.0f, 1.0f, 1.0f);
            GL.Uniform3(uAmbientLightLocation, colour);
            GL.Uniform3(uDiffuseLightLocation, colour);
            GL.Uniform3(uSpecularLightLocation, colour);

            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(this.ClientRectangle);

            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }

            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateRotationY(-0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }

            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }

            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateRotationY(0.025f);
                int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uView, true, ref mView);
            }

            if (e.KeyChar == 'z')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }

            if (e.KeyChar == 'x')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mGroundModel = mGroundModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }

            if (e.KeyChar == 'c')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mModelModel = mModelModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
                mCylinderModel = mCylinderModel * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }

            if (e.KeyChar == 'v')
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mModelModel = mModelModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
                mCylinderModel = mCylinderModel * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            int uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");

            GL.UniformMatrix4(uModel, true, ref mGroundModel);

            Vector3 groundAmbientReflectivity = new Vector3(0.02f, 0.02f, 0.02f);
            GL.Uniform3(uAmbientReflectivityLocation, groundAmbientReflectivity);

            Vector3 groundDiffuseReflectivity = new Vector3(0.01f, 0.01f, 0.01f);
            GL.Uniform3(uDiffuseReflectivityLocation, groundDiffuseReflectivity);

            Vector3 groundSpecularReflectivity = new Vector3(0.4f, 0.4f, 0.4f);
            GL.Uniform3(uSpecularReflectivityLocation, groundSpecularReflectivity);

            float groundShininess = 10f;
            GL.Uniform1(uShininessLocation, groundShininess);

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            Matrix4 m1 = mModelModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m1);

            Vector3 modelAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, modelAmbientReflectivity);

            Vector3 modelDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, modelDiffuseReflectivity);

            Vector3 modelSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, modelSpecularReflectivity);

            float modelShininess = 76.8f;
            GL.Uniform1(uShininessLocation, modelShininess);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mModelModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            
            Matrix4 m2 = mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);

            Vector3 cylinderAmbientReflectivity = new Vector3(0.05375f, 0.05f, 0.06625f);
            GL.Uniform3(uAmbientReflectivityLocation, cylinderAmbientReflectivity);

            Vector3 cylinderDiffuseReflectivity = new Vector3(0.18275f, 0.17f, 0.22525f);
            GL.Uniform3(uDiffuseReflectivityLocation, cylinderDiffuseReflectivity);

            Vector3 cylinderSpecularReflectivity = new Vector3(0.332741f, 0.328634f, 0.346435f);
            GL.Uniform3(uSpecularReflectivityLocation, cylinderSpecularReflectivity);

            float cylinderShininess = 38.4f;
            GL.Uniform1(uShininessLocation, cylinderShininess);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            
            GL.BindVertexArray(0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindVertexArray(0);

            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);

            mShader.Delete();

            base.OnUnload(e);
        }
    }
}