using System;
using System.Collections.Generic;
using Labs.Utility;
using Labs.Lab4;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class Sphere
    {
        public Vector3 mSpherePosition, mSphereVelocity;

        public float mSphereRadius, mSphereMass;
    }

    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {

        }

        private int[] mVBO_IDs = new int[11];
        private int[] mVAO_IDs = new int[6];

        private ShaderUtility mShader;
        private ModelUtility mEmitterBoxModelUtility, mGridBox1ModelUtility, mGridBox2ModelUtility, mSphereOfDoomBoxModelUtility, mSphereModelUtility;
        private Matrix4 mView, mEmitterBoxModel, mGridBox1Model, mGridBox2Model, mSphereOfDoomBoxModel, mWorld;

        private Timer mTimer;
        private float accelerationDueToGravity, coefficientOfRestitution, ellapsedTime, randomEllapsedTime;

        private Random random = new Random();
        private List<Sphere> sphereList = new List<Sphere>();

        Sphere CreateSphereItem()
        {
            Sphere sphereItem = new Sphere();

            if (random.Next(0, 2) == 1)
            {
                sphereItem.mSphereRadius = 0.02F;

                sphereItem.mSphereMass = (float)(0.0012f * ((4 / 3) * Math.PI * Math.Pow(sphereItem.mSphereRadius, 3)));
            }
            else
            {
                sphereItem.mSphereRadius = (float)(0.2 / (100 / 14));

                sphereItem.mSphereRadius = (float)(0.0014f * ((4 / 3) * Math.PI * Math.Pow(sphereItem.mSphereRadius, 3)));
            }

            sphereItem.mSpherePosition = new Vector3(random.Next(-200, 200), random.Next(-200, 200), random.Next(-200, 200));
            sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X / 1000, ((sphereItem.mSpherePosition.Y / 1000) + 0.6f) - sphereItem.mSphereRadius, sphereItem.mSpherePosition.Z / 1000);

            if (sphereItem.mSpherePosition.X > 0)
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X - sphereItem.mSphereRadius, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z);
            }
            else
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X + sphereItem.mSphereRadius, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z);
            }

            if (sphereItem.mSpherePosition.Z > 0)
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z - sphereItem.mSphereRadius);
            }
            else
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z + sphereItem.mSphereRadius);
            }

            sphereItem.mSphereVelocity = new Vector3(random.Next(-100, 101), random.Next(-1000, 1001), random.Next(-1000, 1001));
            sphereItem.mSphereVelocity = new Vector3(sphereItem.mSphereVelocity.X / 1000, (sphereItem.mSphereVelocity.Y / 1000) + 0.6f, sphereItem.mSphereVelocity.Z / 1000);

            return sphereItem;
        }

        void AddToList()
        {
            Sphere sphereItem = CreateSphereItem();

            sphereList.Add(sphereItem);
        }

        protected override void OnLoad(EventArgs e)
        {
            int size;

            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.CullFace(CullFaceMode.Front);

            mShader = new ShaderUtility(@"ACW/Shaders/vPassThrough.vert", @"ACW/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");

            #region Box

            #region Light

            int uLightPositionLocation0 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            int uAmbientLightLocation0 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].AmbientLight");
            int uDiffuseLightLocation0 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].DiffuseLight");
            int uSpecularLightLocation0 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].SpecularLight");
            int uLightPositionLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            int uAmbientLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].AmbientLight");
            int uDiffuseLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].DiffuseLight");
            int uSpecularLightLocation1 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].SpecularLight");
            int uLightPositionLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            int uAmbientLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
            int uDiffuseLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
            int uSpecularLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");

            #endregion

            #region World

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] { 0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0 };

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

            #endregion

            #region EmitterBox

            mEmitterBoxModelUtility = ModelUtility.LoadModel(@"Utility/Models/TopBox.sjg");

            GL.BindVertexArray(mVAO_IDs[1]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mEmitterBoxModelUtility.Vertices.Length * sizeof(float)), mEmitterBoxModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mEmitterBoxModelUtility.Indices.Length * sizeof(float)), mEmitterBoxModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mEmitterBoxModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mEmitterBoxModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #region GridBox1

            mGridBox1ModelUtility = ModelUtility.LoadModel(@"Utility/Models/MiddleBox.sjg");

            GL.BindVertexArray(mVAO_IDs[2]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mGridBox1ModelUtility.Vertices.Length * sizeof(float)), mGridBox1ModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mGridBox1ModelUtility.Indices.Length * sizeof(float)), mGridBox1ModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mGridBox1ModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mGridBox1ModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #region GridBox2

            mGridBox2ModelUtility = ModelUtility.LoadModel(@"Utility/Models/MiddleBox.sjg");

            GL.BindVertexArray(mVAO_IDs[3]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[5]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mGridBox2ModelUtility.Vertices.Length * sizeof(float)), mGridBox2ModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mGridBox2ModelUtility.Indices.Length * sizeof(float)), mGridBox2ModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mGridBox2ModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mGridBox2ModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #region SphereOfDoomBox

            mSphereOfDoomBoxModelUtility = ModelUtility.LoadModel(@"Utility/Models/BottomBox.sjg");

            GL.BindVertexArray(mVAO_IDs[4]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[7]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereOfDoomBoxModelUtility.Vertices.Length * sizeof(float)), mSphereOfDoomBoxModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[8]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereOfDoomBoxModelUtility.Indices.Length * sizeof(float)), mSphereOfDoomBoxModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereOfDoomBoxModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereOfDoomBoxModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #endregion

            #region Sphere

            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/Sphere.bin");

            GL.BindVertexArray(mVAO_IDs[5]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[9]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[10]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #region LightAndCamera

            #region Translations

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            GL.UniformMatrix4(uView, true, ref mView);

            mWorld = Matrix4.CreateTranslation(0, 1.5f, -2f);

            mEmitterBoxModel = Matrix4.CreateTranslation(0, 0.6f, 0);

            mGridBox1Model = Matrix4.CreateTranslation(0, 0.2f, 0);

            mGridBox2Model = Matrix4.CreateTranslation(0, -0.2f, 0);

            mSphereOfDoomBoxModel = Matrix4.CreateTranslation(0, -0.6f, 0);

            #endregion

            Vector4 eyePosition = Vector4.Transform(new Vector4(0, 0, 0, 1), mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);

            Vector4 lightPosition0 = Vector4.Transform(new Vector4(5, 5, -6, 1), mView);
            GL.Uniform4(uLightPositionLocation0, lightPosition0);

            Vector4 lightPosition1 = Vector4.Transform(new Vector4(0, 5, -1, 1), mView);
            GL.Uniform4(uLightPositionLocation1, lightPosition1);

            Vector4 lightPosition2 = Vector4.Transform(new Vector4(-5, 5, -6, 1), mView);
            GL.Uniform4(uLightPositionLocation2, lightPosition2);

            Vector3 colour0 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation0, colour0);
            GL.Uniform3(uDiffuseLightLocation0, colour0);
            GL.Uniform3(uSpecularLightLocation0, colour0);

            Vector3 colour1 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation1, colour1);
            GL.Uniform3(uDiffuseLightLocation1, colour1);
            GL.Uniform3(uSpecularLightLocation1, colour1);

            Vector3 colour2 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation2, colour2);
            GL.Uniform3(uDiffuseLightLocation2, colour2);
            GL.Uniform3(uSpecularLightLocation2, colour2);

            #endregion

            ;

            accelerationDueToGravity = -9.81f;
            coefficientOfRestitution = 0.75f;

            ellapsedTime = 0;
            randomEllapsedTime = random.Next(0, 1001);
            randomEllapsedTime = randomEllapsedTime / 1000;
            
            AddToList();

            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
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
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }

            if (e.KeyChar == 'c')
            {
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
            }

            if (e.KeyChar == 'r')
            {
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationX(-0.025f) * translation;
            }

            if (e.KeyChar == 'f')
            {
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationX(0.025f) * translation;
            }

            base.OnKeyPress(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(this.ClientRectangle);

            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }

            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timestep = mTimer.GetElapsedSeconds();

            ellapsedTime = ellapsedTime + timestep;

            for (int i = 0; i < sphereList.Count; i++)
            {
                sphereList[i].mSphereVelocity.Y = sphereList[i].mSphereVelocity.Y + accelerationDueToGravity * timestep;

                Vector3 mPreviousSpherePosition = sphereList[i].mSpherePosition;

                sphereList[i].mSpherePosition = sphereList[i].mSpherePosition + sphereList[i].mSphereVelocity * timestep;

                for (int j = 0; j < i; j++)
                {
                    if (j != i && (Math.Sqrt(Math.Pow((sphereList[j].mSpherePosition.X - sphereList[i].mSpherePosition.X), 2) + Math.Pow((sphereList[j].mSpherePosition.Y - sphereList[i].mSpherePosition.Y), 2) + Math.Pow((sphereList[j].mSpherePosition.Z - sphereList[i].mSpherePosition.Z), 2)) <= (sphereList[j].mSphereRadius + sphereList[i].mSphereRadius)))
                    {
                        Vector3 tempVelocity = sphereList[j].mSphereVelocity;

                        sphereList[j].mSphereVelocity = (Vector3.Multiply(sphereList[i].mSphereVelocity, (sphereList[j].mSphereMass - sphereList[i].mSphereMass) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass)) + Vector3.Multiply(sphereList[j].mSphereVelocity, (sphereList[i].mSphereMass * 2) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass))) * coefficientOfRestitution;
                        sphereList[i].mSphereVelocity = (Vector3.Multiply(tempVelocity, (sphereList[i].mSphereMass - sphereList[j].mSphereMass) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass)) + Vector3.Multiply(sphereList[i].mSphereVelocity, (sphereList[j].mSphereMass * 2) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass))) * coefficientOfRestitution;

                        sphereList[i].mSpherePosition = mPreviousSpherePosition;
                    }
                }

                if ((sphereList[i].mSpherePosition.X + (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().X)) >= 0.2 || (sphereList[i].mSpherePosition.X - (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().X)) <= -0.2)
                {
                    Vector3 normal = new Vector3(-1, 0, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;

                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }

                if ((sphereList[i].mSpherePosition.Y + (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().Y)) <= -0.8)
                {
                    Vector3 tempPosition = sphereList[i].mSpherePosition;
                    Vector3 tempVelocity = sphereList[i].mSphereVelocity;

                    sphereList[i].mSphereVelocity.X = tempVelocity.Y;
                    sphereList[i].mSphereVelocity.Y = tempVelocity.X;

                    sphereList[i].mSpherePosition.Y = 0.6f - sphereList[i].mSpherePosition.X;
                    sphereList[i].mSpherePosition.X = 0.2f - sphereList[i].mSphereRadius;
                }

                if ((sphereList[i].mSpherePosition.Y - (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().Y)) >= 0.8)
                {
                    Vector3 normal = new Vector3(0, -1, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                    
                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }

                if ((sphereList[i].mSpherePosition.Z + (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().Z)) >= 0.2 || (sphereList[i].mSpherePosition.Z - (sphereList[i].mSphereRadius / mEmitterBoxModel.ExtractScale().Z)) <= -0.2)
                {
                    Vector3 normal = new Vector3(0, 0, -1);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;

                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }
            }

            if (ellapsedTime > randomEllapsedTime)
            {
                AddToList();

                Console.WriteLine(sphereList.Count);

                ellapsedTime = 0;
                randomEllapsedTime = random.Next(0, 1001);
                randomEllapsedTime = randomEllapsedTime / 1000;
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            int uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");

            #region World

            GL.UniformMatrix4(uModel, true, ref mWorld);

            Vector3 worldAmbientReflectivity = new Vector3(0, 0, 0);
            GL.Uniform3(uAmbientReflectivityLocation, worldAmbientReflectivity);

            Vector3 worldDiffuseReflectivity = new Vector3(0, 0, 0);
            GL.Uniform3(uDiffuseReflectivityLocation, worldDiffuseReflectivity);

            Vector3 worldSpecularReflectivity = new Vector3(0, 0, 0);
            GL.Uniform3(uSpecularReflectivityLocation, worldSpecularReflectivity);

            float worldShininess = 0f;
            GL.Uniform1(uShininessLocation, worldShininess);



            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            #endregion

            #region Box

            GL.Enable(EnableCap.CullFace);

            #region EmitterBox

            Matrix4 m1 = mEmitterBoxModel * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m1);

            Vector3 emitterBoxAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, emitterBoxAmbientReflectivity);

            Vector3 emitterBoxDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, emitterBoxDiffuseReflectivity);

            Vector3 emitterBoxSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, emitterBoxSpecularReflectivity);

            float emitterBoxShininess = 76.8f;
            GL.Uniform1(uShininessLocation, emitterBoxShininess);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mEmitterBoxModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region GridBox1

            Matrix4 m2 = mGridBox1Model * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);

            Vector3 GridBox1AmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, GridBox1AmbientReflectivity);

            Vector3 GridBox1DiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, GridBox1DiffuseReflectivity);

            Vector3 GridBox1SpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, GridBox1SpecularReflectivity);

            float GridBox1Shininess = 76.8f;
            GL.Uniform1(uShininessLocation, GridBox1Shininess);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mGridBox1ModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region GridBox2

            Matrix4 m3 = mGridBox2Model * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m3);

            Vector3 GridBox2AmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, GridBox2AmbientReflectivity);

            Vector3 GridBox2DiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, GridBox2DiffuseReflectivity);

            Vector3 GridBox2SpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, GridBox2SpecularReflectivity);

            float GridBox2Shininess = 76.8f;
            GL.Uniform1(uShininessLocation, GridBox2Shininess);

            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, mGridBox2ModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region SphereOfDoomBox

            Matrix4 m4 = mSphereOfDoomBoxModel * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m4);

            Vector3 SphereOfDoomBoxAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, SphereOfDoomBoxAmbientReflectivity);

            Vector3 SphereOfDoomBoxDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, SphereOfDoomBoxDiffuseReflectivity);

            Vector3 SphereOfDoomBoxSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, SphereOfDoomBoxSpecularReflectivity);

            float SphereOfDoomBoxShininess = 76.8f;
            GL.Uniform1(uShininessLocation, SphereOfDoomBoxShininess);

            GL.BindVertexArray(mVAO_IDs[4]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereOfDoomBoxModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            GL.Disable(EnableCap.CullFace);

            #endregion

            #region Sphere

            for (int i = 0; i < sphereList.Count; i++)
            {
                Matrix4 m5 = Matrix4.CreateScale(sphereList[i].mSphereRadius) * Matrix4.CreateTranslation(sphereList[i].mSpherePosition) * mWorld;
                uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
                GL.UniformMatrix4(uModel, true, ref m5);

                Vector3 SphereAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
                GL.Uniform3(uAmbientReflectivityLocation, SphereAmbientReflectivity);

                Vector3 SphereDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
                GL.Uniform3(uDiffuseReflectivityLocation, SphereDiffuseReflectivity);

                Vector3 SphereSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
                GL.Uniform3(uSpecularReflectivityLocation, SphereSpecularReflectivity);

                float SphereShininess = 76.8f;
                GL.Uniform1(uShininessLocation, SphereShininess);

                GL.BindVertexArray(mVAO_IDs[5]);
                GL.DrawElements(PrimitiveType.Triangles, mSphereModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            #endregion

            this.SwapBuffers();

            base.OnRenderFrame(e);
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
