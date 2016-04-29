using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Labs.Utility;
using Labs.Lab4;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class Cylinder
    {
        public Vector3 mCylinderPosition, mCylinderRotation, mCylinderScale;


        public float mCylinderRadius;
    }

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

        #region Variables

        enum Camera
        {
            Static,
            User,
            Moving,
            Follow
        }

        private int[] mVBO_IDs = new int[15];
        private int[] mVAO_IDs = new int[8];

        private ShaderUtility mShader;
        private ModelUtility mEmitterBoxModelUtility, mGridBox1ModelUtility, mGridBox2ModelUtility, mSphereOfDoomBoxModelUtility, mSphereOfDoomModelUtility, mCylinderModelUtility, mSphereModelUtility;
        private Matrix4 mView, mEmitterBoxModel, mGridBox1Model, mGridBox2Model, mSphereOfDoomBoxModel, mWorld, mWorld2;
        private int mTexture_ID;

        private Sphere sphereOfDoom = new Sphere();
        private Cylinder[] cylinderArray = new Cylinder[6];
        private List<Sphere> sphereList = new List<Sphere>();

        private Random random = new Random();

        Camera camera;
        private int randomBall;

        private Timer mTimer;
        private bool onResizeBool, simulationBool, speedBool;
        private float accelerationDueToGravity, coefficientOfRestitution, ellapsedTime, randomEllapsedTime;

        #endregion

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
                sphereItem.mSphereRadius = 0.028f;

                sphereItem.mSphereMass = (float)(0.0014f * ((4 / 3) * Math.PI * Math.Pow(sphereItem.mSphereRadius, 3)));
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

            sphereItem.mSphereVelocity = new Vector3(random.Next(-1000, 1001), random.Next(-1000, 1001), random.Next(-1000, 1001));
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

            #region SphereOfDoom

            mSphereOfDoomModelUtility = ModelUtility.LoadModel(@"Utility/Models/Sphere.bin");

            GL.BindVertexArray(mVAO_IDs[5]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[9]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereOfDoomModelUtility.Vertices.Length * sizeof(float)), mSphereOfDoomModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[10]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereOfDoomModelUtility.Indices.Length * sizeof(float)), mSphereOfDoomModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereOfDoomModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if (mSphereOfDoomModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.BindVertexArray(0);

            #endregion

            #region Cylinder

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/Cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[6]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[11]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[12]);
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

            #endregion

            #region Sphere

            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/Sphere.bin");

            GL.BindVertexArray(mVAO_IDs[7]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[13]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[14]);
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

            camera = Camera.User;

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

            accelerationDueToGravity = -9.81f;
            coefficientOfRestitution = 0.75f;

            ellapsedTime = 0;
            randomEllapsedTime = random.Next(0, 1001);
            randomEllapsedTime = randomEllapsedTime / 1000;

            #region Initialising

            #region Sphere

            sphereOfDoom.mSpherePosition = new Vector3(0, -0.6f, 0);
            sphereOfDoom.mSphereRadius = 0.12f;

            #endregion

            #region Cylinder

            for (int i = 0; i < cylinderArray.Length; i++)
            {
                cylinderArray[i] = new Cylinder();
            }

            cylinderArray[0].mCylinderPosition = new Vector3(0, (float)((0.2 / (100 / 45)) + 0.2), 0);
            cylinderArray[1].mCylinderPosition = new Vector3(0.1f, (float)(-(0.2 / (100 / 45)) + 0.2), 0);
            cylinderArray[2].mCylinderPosition = new Vector3(-0.1f, (float)(-(0.2 / (100 / 45)) + 0.2), 0);
            cylinderArray[3].mCylinderPosition = new Vector3(0, 0.2f, 0);
            cylinderArray[4].mCylinderPosition = new Vector3(0, -0.2f, 0);
            cylinderArray[5].mCylinderPosition = new Vector3(0, -0.2f, 0);

            for (int i = 0; i < 3; i++)
            {
                cylinderArray[i].mCylinderRotation = new Vector3((float)Math.PI / 2, 0, 0);
                cylinderArray[i].mCylinderRadius = 0.03f;
                cylinderArray[i].mCylinderScale = new Vector3(1, 6.5f, 1);
            }

            cylinderArray[3].mCylinderRotation = new Vector3((float)Math.PI / 2, (float)Math.PI / 2, 0);
            cylinderArray[4].mCylinderRotation = new Vector3((float)(2 * Math.PI / 3), (float)-Math.PI / 4, 0);
            cylinderArray[5].mCylinderRotation = new Vector3((float)Math.PI / 2, (float)Math.PI / 4, 0);

            cylinderArray[3].mCylinderRadius = 0.06f;
            cylinderArray[4].mCylinderRadius = 0.06f;
            cylinderArray[5].mCylinderRadius = 0.04f;

            cylinderArray[3].mCylinderScale = new Vector3(1, 3.25f, 1);
            cylinderArray[4].mCylinderScale = new Vector3(1, 4.25f, 1);
            cylinderArray[5].mCylinderScale = new Vector3(1, 6, 1);

            #region Texturing

            Bitmap TextureBitmap;
            BitmapData TextureData;

            string filepath = @"ACW/MaxResDefault.jpg";

            if (System.IO.File.Exists(filepath))
            {
                TextureBitmap = new Bitmap(filepath);
                TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GenTextures(1, out mTexture_ID);
            GL.BindTexture(TextureTarget.Texture2D, mTexture_ID);
            GL.TexImage2D(TextureTarget.Texture2D,
            0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height,
            0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
            PixelType.UnsignedByte, TextureData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
            TextureBitmap.UnlockBits(TextureData);

            #endregion

            #endregion

            AddToList();

            #endregion

            onResizeBool = false;
            simulationBool = true;
            speedBool = false;

            mWorld2 = mWorld;

            mTimer = new Timer();
            mTimer.Start();

            base.OnLoad(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (camera == Camera.User)
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
            }

            if (e.KeyChar == '1')
            {
                mWorld = mWorld2;
                camera = Camera.Static;
            }

            if (e.KeyChar == '2')
            {
                mWorld = mWorld2;
                camera = Camera.User;
            }

            if (e.KeyChar == '3')
            {
                mWorld = mWorld2;
                camera = Camera.Moving;
            }

            if (e.KeyChar == '4')
            {
                mWorld = mWorld2;
                randomBall = random.Next(0, (sphereList.Count + 1));

                camera = Camera.Follow;
            }

            if (e.KeyChar == 'x')
            {
                if (simulationBool)
                {
                    simulationBool = false;
                }
                else
                {
                    simulationBool = true;
                }
            }

            if (e.KeyChar == 'q')
            {
                if (speedBool)
                {
                    speedBool = false;
                }
                else
                {
                    speedBool = true;
                }
            }

            base.OnKeyPress(e);
        }

        protected override void OnResize(EventArgs e)
        {
            onResizeBool = true;

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
            if (camera == Camera.Moving)
            {
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }

            if (camera == Camera.Follow)
            {
                try
                {
                    mWorld = mWorld * Matrix4.CreateTranslation(-(sphereList[randomBall].mSphereVelocity * (float)0.02));
                }
                catch
                {

                }
            }

            if(onResizeBool)
            {
                onResizeBool = false;

                mTimer.GetElapsedSeconds();
            }

            float timestep = mTimer.GetElapsedSeconds();

            ellapsedTime = ellapsedTime + timestep;

            for (int i = 0; i < sphereList.Count; i++)
            {
                if (simulationBool)
                {
                    if (speedBool)
                    {
                        sphereList[i].mSphereVelocity.Y = (sphereList[i].mSphereVelocity.Y + accelerationDueToGravity * timestep) / 2;
                    }
                    else
                    {
                        sphereList[i].mSphereVelocity.Y = sphereList[i].mSphereVelocity.Y + accelerationDueToGravity * timestep;
                    }
                }

                Vector3 mPreviousSpherePosition = sphereList[i].mSpherePosition;

                sphereList[i].mSpherePosition = sphereList[i].mSpherePosition + sphereList[i].mSphereVelocity * timestep;

                #region SphereCollide

                for (int j = 0; j < i; j++)
                {
                    if (j != i && (Math.Sqrt(Math.Pow((sphereList[i].mSpherePosition.X - sphereList[j].mSpherePosition.X), 2) + Math.Pow((sphereList[i].mSpherePosition.Y - sphereList[j].mSpherePosition.Y), 2) + Math.Pow((sphereList[i].mSpherePosition.Z - sphereList[j].mSpherePosition.Z), 2)) <= (sphereList[i].mSphereRadius + sphereList[j].mSphereRadius)))
                    {
                        Vector3 temporaryVelocity = sphereList[i].mSphereVelocity;

                        sphereList[i].mSphereVelocity = (Vector3.Multiply(temporaryVelocity, (sphereList[i].mSphereMass - sphereList[j].mSphereMass) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass)) + Vector3.Multiply(sphereList[i].mSphereVelocity, (sphereList[j].mSphereMass * 2) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass))) * coefficientOfRestitution;
                        sphereList[j].mSphereVelocity = (Vector3.Multiply(sphereList[i].mSphereVelocity, (sphereList[j].mSphereMass - sphereList[i].mSphereMass) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass)) + Vector3.Multiply(sphereList[j].mSphereVelocity, (sphereList[i].mSphereMass * 2) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass))) * coefficientOfRestitution;

                        sphereList[i].mSpherePosition = mPreviousSpherePosition;
                    }
                }

                #endregion

                #region CylinderCollide

                for (int j = 0; j < cylinderArray.Length; j++)
                {

                }

                #endregion

                #region SphereOfDoomCollide

                int uLightPositionLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
                int uAmbientLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
                int uDiffuseLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
                int uSpecularLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");

                if (Math.Sqrt(Math.Pow((sphereList[i].mSpherePosition.X - sphereOfDoom.mSpherePosition.X), 2) + Math.Pow((sphereList[i].mSpherePosition.Y - sphereOfDoom.mSpherePosition.Y), 2) + Math.Pow((sphereList[i].mSpherePosition.Z - sphereOfDoom.mSpherePosition.Z), 2)) <= (sphereList[i].mSphereRadius + sphereOfDoom.mSphereRadius))
                {
                    Vector4 lightPosition2 = Vector4.Transform(new Vector4(sphereList[i].mSpherePosition, 1), mView);
                    GL.Uniform4(uLightPositionLocation4, lightPosition2);

                    Vector3 colour4 = new Vector3(1, 0, 0);
                    GL.Uniform3(uAmbientLightLocation4, colour4);
                    GL.Uniform3(uDiffuseLightLocation4, colour4);
                    GL.Uniform3(uSpecularLightLocation4, colour4);
                }
                else
                {
                    Vector3 colour4 = new Vector3(0, 0, 0);
                    GL.Uniform3(uAmbientLightLocation4, colour4);
                    GL.Uniform3(uDiffuseLightLocation4, colour4);
                    GL.Uniform3(uSpecularLightLocation4, colour4);
                }

                #endregion

                #region WallCollide

                if (sphereList[i].mSpherePosition.X + sphereList[i].mSphereRadius >= 0.2 || sphereList[i].mSpherePosition.X - sphereList[i].mSphereRadius <= -0.2)
                {
                    Vector3 normal = new Vector3(-1, 0, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;

                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }

                if (sphereList[i].mSpherePosition.Y + sphereList[i].mSphereRadius <= -0.8)
                {
                    Vector3 temporaryPosition = sphereList[i].mSpherePosition;
                    Vector3 temporaryVelocity = sphereList[i].mSphereVelocity;

                    sphereList[i].mSphereVelocity.X = temporaryVelocity.Y;
                    sphereList[i].mSphereVelocity.Y = temporaryVelocity.X;

                    sphereList[i].mSpherePosition.Y = 0.6f - sphereList[i].mSpherePosition.X;
                    sphereList[i].mSpherePosition.X = 0.2f - sphereList[i].mSphereRadius;
                }

                if (sphereList[i].mSpherePosition.Y - sphereList[i].mSphereRadius >= 0.8)
                {
                    Vector3 normal = new Vector3(0, -1, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                    
                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }

                if (sphereList[i].mSpherePosition.Z + sphereList[i].mSphereRadius >= 0.2 || sphereList[i].mSpherePosition.Z - sphereList[i].mSphereRadius <= -0.2)
                {
                    Vector3 normal = new Vector3(0, 0, -1);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;

                    sphereList[i].mSpherePosition = mPreviousSpherePosition;
                }

                #endregion

                if (!simulationBool)
                {
                    if (speedBool)
                    {
                        sphereList[i].mSphereVelocity.Y = (sphereList[i].mSphereVelocity.Y + accelerationDueToGravity * timestep) / 2;
                    }
                    else
                    {
                        sphereList[i].mSphereVelocity.Y = sphereList[i].mSphereVelocity.Y + accelerationDueToGravity * timestep;
                    }
                }
            }

            if (ellapsedTime > randomEllapsedTime)
            {
                AddToList();

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

            #region SphereOfDoom

            Matrix4 m5 = Matrix4.CreateScale(sphereOfDoom.mSphereRadius) * Matrix4.CreateTranslation(sphereOfDoom.mSpherePosition) * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m5);

            Vector3 SphereOfDoomAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, SphereOfDoomAmbientReflectivity);

            Vector3 SphereOfDoomDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, SphereOfDoomDiffuseReflectivity);

            Vector3 SphereOfDoomSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, SphereOfDoomSpecularReflectivity);

            float SphereOfDoomShininess = 76.8f;
            GL.Uniform1(uShininessLocation, SphereOfDoomShininess);

            GL.BindVertexArray(mVAO_IDs[7]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereOfDoomModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region Cylinder

            for (int i = 0; i < cylinderArray.Length; i++)
            {
                Matrix4 m6 = Matrix4.CreateScale(cylinderArray[i].mCylinderScale) * Matrix4.CreateRotationX(cylinderArray[i].mCylinderRotation.X) * Matrix4.CreateRotationY(cylinderArray[i].mCylinderRotation.Y) * Matrix4.CreateScale(cylinderArray[i].mCylinderRadius) * Matrix4.CreateTranslation(cylinderArray[i].mCylinderPosition) * mWorld;
                uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
                GL.UniformMatrix4(uModel, true, ref m6);

                Vector3 CylinderAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
                GL.Uniform3(uAmbientReflectivityLocation, CylinderAmbientReflectivity);

                Vector3 CylinderDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
                GL.Uniform3(uDiffuseReflectivityLocation, CylinderDiffuseReflectivity);

                Vector3 CylinderSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
                GL.Uniform3(uSpecularReflectivityLocation, CylinderSpecularReflectivity);

                float CylinderShininess = 76.8f;
                GL.Uniform1(uShininessLocation, CylinderShininess);

                GL.BindVertexArray(mVAO_IDs[6]);
                GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            #endregion

            #region Sphere

            for (int i = 0; i < sphereList.Count; i++)
            {
                Matrix4 m7 = Matrix4.CreateScale(sphereList[i].mSphereRadius) * Matrix4.CreateTranslation(sphereList[i].mSpherePosition) * mWorld;
                uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
                GL.UniformMatrix4(uModel, true, ref m7);

                Vector3 SphereAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
                GL.Uniform3(uAmbientReflectivityLocation, SphereAmbientReflectivity);

                Vector3 SphereDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
                GL.Uniform3(uDiffuseReflectivityLocation, SphereDiffuseReflectivity);

                Vector3 SphereSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
                GL.Uniform3(uSpecularReflectivityLocation, SphereSpecularReflectivity);

                float SphereShininess = 76.8f;
                GL.Uniform1(uShininessLocation, SphereShininess);

                GL.BindVertexArray(mVAO_IDs[7]);
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
