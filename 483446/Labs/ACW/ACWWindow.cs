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
    //This is the class used to describe a cylinder
    public class Cylinder
    {
        public Vector3 mCylinderPosition, mCylinderRotation, mCylinderScale;

        public float mCylinderRadius;
    }

    //This is the class used to describe a sphere
    public class Sphere
    {
        public Vector3 mSpherePosition, mSphereVelocity;

        public float mSphereRadius, mSphereMass;

        public bool mSphereBool;
    }

    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(800, 600, GraphicsMode.Default, "Assessed Coursework", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {

        }

        #region Variables

        //This is the enum used to describe a camera
        enum Camera
        {
            Static,
            User,
            Mouse,
            Moving,
            Follow
        }

        //This is the arrays used to store the vertex arrays and bufferes
        private int[] mVAO_IDs = new int[8];
        private int[] mVBO_IDs = new int[15];

        //These are the variables used to store the models and shaders
        private ShaderUtility mShader, mShader2;
        private ModelUtility mEmitterBoxModelUtility, mGridBox1ModelUtility, mGridBox2ModelUtility, mSphereOfDoomBoxModelUtility, mSphereOfDoomModelUtility, mCylinderModelUtility, mSphereModelUtility;
        private Matrix4 mView, mEmitterBoxModel, mGridBox1Model, mGridBox2Model, mSphereOfDoomBoxModel, mWorld, mWorld2;
        //private int mTexture_ID;

        //These are the variables used to store the lists of spheres and cylinders
        private Sphere sphereOfDoom = new Sphere();
        private Cylinder[] cylinderArray = new Cylinder[6];
        private List<Sphere> sphereList = new List<Sphere>();

        //This is a variable that holds a random array of numbers
        private Random random = new Random();

        //These are variables used to control the current state of the camera
        Camera camera;
        private int randomSphere;

        //These are the variables used to control the simulation animation
        private Timer mTimer;
        private bool simulationBool;
        private Vector3 accelerationDueToGravity, mouse;
        private float coefficientOfRestitution, ellapsedTime, randomEllapsedTime;

        #endregion

        Sphere CreateSphereItem()
        {
            //This creates an instance of the sphere class
            Sphere sphereItem = new Sphere();

            //This divides the spheres up randomly into large and small spheres
            if (random.Next(0, 2) == 1)
            {
                //This is a small sphere
                sphereItem.mSphereBool = true;

                //This sets the radius of the sphere
                sphereItem.mSphereRadius = (float)((0.4 * 5) / 100);

                //This sets the mass of the sphere
                sphereItem.mSphereMass = (float)(0.0012 * ((4 / 3) * (Math.PI * Math.Pow(5, 3))));
            }
            else
            {
                //This is a large sphere
                sphereItem.mSphereBool = false;

                //This sets the radius of the sphere
                sphereItem.mSphereRadius = (float)((0.4 * 7) / 100);

                //This sets the mass of the sphere
                sphereItem.mSphereMass = (float)(0.0014 * ((4 / 3) * (Math.PI * Math.Pow(7, 3))));
            }

            //This sets the position of the sphere to a random position between -0.2 and 0.2
            sphereItem.mSpherePosition = new Vector3(random.Next(-200, 201), random.Next(-200, 201), random.Next(-200, 201));
            sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X / 1000, (sphereItem.mSpherePosition.Y / 1000) + 0.6f, sphereItem.mSpherePosition.Z / 1000);

            if (sphereItem.mSpherePosition.X > 0)
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X - sphereItem.mSphereRadius, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z);
            }
            else
            {
                if (sphereItem.mSpherePosition.X < 0)
                {
                    sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X + sphereItem.mSphereRadius, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z);
                }
            }

            if (sphereItem.mSpherePosition.Y > 0)
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y - sphereItem.mSphereRadius, sphereItem.mSpherePosition.Z);
            }
            else
            {
                if (sphereItem.mSpherePosition.Y < 0)
                {
                    sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y + sphereItem.mSphereRadius, sphereItem.mSpherePosition.Z);
                }
            }

            if (sphereItem.mSpherePosition.Z > 0)
            {
                sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z - sphereItem.mSphereRadius);
            }
            else
            {
                if (sphereItem.mSpherePosition.Z < 0)
                {
                    sphereItem.mSpherePosition = new Vector3(sphereItem.mSpherePosition.X, sphereItem.mSpherePosition.Y, sphereItem.mSpherePosition.Z + sphereItem.mSphereRadius);
                }
            }

            //This sets the velocity of the sphere to a random velocity between -1 and 1
            sphereItem.mSphereVelocity = new Vector3(random.Next(-1000, 1001), random.Next(-1000, 1001), random.Next(-1000, 1001));
            sphereItem.mSphereVelocity = new Vector3(sphereItem.mSphereVelocity.X / 1000, (sphereItem.mSphereVelocity.Y / 1000) + 0.6f, sphereItem.mSphereVelocity.Z / 1000);

            return sphereItem;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int size;

            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.CullFace(CullFaceMode.Front);

            //This loads the shader programs
            mShader = new ShaderUtility(@"ACW/Shaders/vPassThrough.vert", @"ACW/Shaders/fLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            mShader2 = new ShaderUtility(@"ACW/Shaders/vPassThrough2.vert", @"ACW/Shaders/fLighting2.frag");
            //GL.UseProgram(mShader2.ShaderProgramID);

            #region ShaderVariables

            //These are variables used to load values into the shader program
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vPositionLocation2 = GL.GetAttribLocation(mShader2.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int vNormalLocation2 = GL.GetAttribLocation(mShader2.ShaderProgramID, "vNormal");
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            int uView2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uView");
            int uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            int uEyePositionLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uEyePosition");

            #endregion

            #region World

            //This generates vertex array objects
            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            //These are vertices used to draw the object
            float[] vertices = new float[] { 0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0,
                                             0, 0, 0, 0, 0, 0 };

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[0]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region Box

            #region EmitterBox

            //This loads the top box model
            mEmitterBoxModelUtility = ModelUtility.LoadModel(@"Utility/Models/TopBox.sjg");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[1]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mEmitterBoxModelUtility.Vertices.Length * sizeof(float)), mEmitterBoxModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mEmitterBoxModelUtility.Indices.Length * sizeof(float)), mEmitterBoxModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mEmitterBoxModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mEmitterBoxModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region GridBox1

            //This loads the top box model
            mGridBox1ModelUtility = ModelUtility.LoadModel(@"Utility/Models/MiddleBox.sjg");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[2]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mGridBox1ModelUtility.Vertices.Length * sizeof(float)), mGridBox1ModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mGridBox1ModelUtility.Indices.Length * sizeof(float)), mGridBox1ModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mGridBox1ModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mGridBox1ModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region GridBox2

            //This loads the top box model
            mGridBox2ModelUtility = ModelUtility.LoadModel(@"Utility/Models/MiddleBox.sjg");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[3]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[5]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mGridBox2ModelUtility.Vertices.Length * sizeof(float)), mGridBox2ModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mGridBox2ModelUtility.Indices.Length * sizeof(float)), mGridBox2ModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mGridBox2ModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mGridBox2ModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region SphereOfDoomBox

            //This loads the top box model
            mSphereOfDoomBoxModelUtility = ModelUtility.LoadModel(@"Utility/Models/BottomBox.sjg");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[4]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[7]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereOfDoomBoxModelUtility.Vertices.Length * sizeof(float)), mSphereOfDoomBoxModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[8]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereOfDoomBoxModelUtility.Indices.Length * sizeof(float)), mSphereOfDoomBoxModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mSphereOfDoomBoxModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mSphereOfDoomBoxModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #endregion

            #region Contents

            #region SphereOfDoom

            //This loads the top box model
            mSphereOfDoomModelUtility = ModelUtility.LoadModel(@"Utility/Models/Sphere.bin");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[5]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[9]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereOfDoomModelUtility.Vertices.Length * sizeof(float)), mSphereOfDoomModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[10]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereOfDoomModelUtility.Indices.Length * sizeof(float)), mSphereOfDoomModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mSphereOfDoomModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mSphereOfDoomModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region Cylinder

            //This loads the top box model
            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/Cylinder.bin");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[6]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[11]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[12]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation2);
            GL.VertexAttribPointer(vNormalLocation2, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation2);
            GL.VertexAttribPointer(vPositionLocation2, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #region Sphere

            //This loads the top box model
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/Sphere.bin");

            //This bindes the vertices to the vertex array
            GL.BindVertexArray(mVAO_IDs[7]);

            //This binds the vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[13]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModelUtility.Vertices.Length * sizeof(float)), mSphereModelUtility.Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[14]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModelUtility.Indices.Length * sizeof(float)), mSphereModelUtility.Indices, BufferUsageHint.StaticDraw);

            //This enables the normals
            GL.EnableVertexAttribArray(vNormalLocation2);
            GL.VertexAttribPointer(vNormalLocation2, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the vertices are loaded correctly
            if (mSphereModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            //This ensures the indices are loaded correctly
            if (mSphereModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            //This enables the positions
            GL.EnableVertexAttribArray(vPositionLocation2);
            GL.VertexAttribPointer(vPositionLocation2, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //This clears the vertex array
            GL.BindVertexArray(0);

            #endregion

            #endregion

            #region Light/Camera/Translations

            //This is the initial state of the camera
            camera = Camera.User;

            #region Translations

            //This is the view from the camera
            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            GL.UniformMatrix4(uView, true, ref mView);
            GL.UniformMatrix4(uView2, true, ref mView);

            //This is the current state of the world
            mWorld = Matrix4.CreateTranslation(0, 1.5f, -2f);

            //This is the initial state of the top box
            mEmitterBoxModel = Matrix4.CreateTranslation(0, 0.6f, 0);

            //This is the initial state of the upper middle box
            mGridBox1Model = Matrix4.CreateTranslation(0, 0.2f, 0);

            //This is the initial state of the lower middle box
            mGridBox2Model = Matrix4.CreateTranslation(0, -0.2f, 0);

            //This is the initial state of the bottom box
            mSphereOfDoomBoxModel = Matrix4.CreateTranslation(0, -0.6f, 0);

            #endregion

            //This is the initial position of the 'eye'
            Vector4 eyePosition = Vector4.Transform(new Vector4(0, 0, 0, 1), mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);
            GL.Uniform4(uEyePositionLocation2, eyePosition);

            #region LightVariables

            //These are the variables used to create the lighting
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
            int uLightPositionLocation = GL.GetUniformLocation(mShader2.ShaderProgramID, "uLight.Position");
            int uAmbientLightLocation = GL.GetUniformLocation(mShader2.ShaderProgramID, "uLight.AmbientLight");
            int uDiffuseLightLocation = GL.GetUniformLocation(mShader2.ShaderProgramID, "uLight.DiffuseLight");
            int uSpecularLightLocation = GL.GetUniformLocation(mShader2.ShaderProgramID, "uLight.SpecularLight");

            #endregion

            //This is the position of the first light
            Vector4 lightPosition0 = Vector4.Transform(new Vector4(5, 5, -6, 1), mView);
            GL.Uniform4(uLightPositionLocation0, lightPosition0);

            //This is the position of the second light
            Vector4 lightPosition1 = Vector4.Transform(new Vector4(0, 5, -1, 1), mView);
            GL.Uniform4(uLightPositionLocation1, lightPosition1);

            //This is the position of the third light
            Vector4 lightPosition2 = Vector4.Transform(new Vector4(-5, 5, -6, 1), mView);
            GL.Uniform4(uLightPositionLocation2, lightPosition2);

            //This is the position of the first light in the second shader
            Vector4 lightPosition = Vector4.Transform(new Vector4(5, 5, -6, 1), mView);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            //This is the colour of the first light
            Vector3 colour0 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation0, colour0);
            GL.Uniform3(uDiffuseLightLocation0, colour0);
            GL.Uniform3(uSpecularLightLocation0, colour0);

            //This is the colour of the second light
            Vector3 colour1 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation1, colour1);
            GL.Uniform3(uDiffuseLightLocation1, colour1);
            GL.Uniform3(uSpecularLightLocation1, colour1);

            //This is the colour of the third light
            Vector3 colour2 = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation2, colour2);
            GL.Uniform3(uDiffuseLightLocation2, colour2);
            GL.Uniform3(uSpecularLightLocation2, colour2);

            //This is the colour of the first light in the second shader
            Vector3 colour = new Vector3(1, 1, 1);
            GL.Uniform3(uAmbientLightLocation, colour);
            GL.Uniform3(uDiffuseLightLocation, colour);
            GL.Uniform3(uSpecularLightLocation, colour);

            #endregion

            #region Initialising

            //These variables are used to simulate physics in the animation
            accelerationDueToGravity = new Vector3(0, (float)((0.4 * (-9.81 * 100)) / 100), 0);
            coefficientOfRestitution = 0.75f;

            //These variables are used to add spheres to the simulation
            ellapsedTime = 0;
            randomEllapsedTime = random.Next(0, 1001);
            randomEllapsedTime = randomEllapsedTime / 1000;

            #region SphereOfDoom

            //These variables dictate the position and size of the sphere of doom
            sphereOfDoom.mSpherePosition = new Vector3(0, -0.6f, 0);
            sphereOfDoom.mSphereRadius = (float)((0.4 * 30) / 100);

            #endregion

            #region Cylinder

            //These variables dictate the size, rotation and position of the cylinders
            for (int i = 0; i < cylinderArray.Length; i++)
            {
                cylinderArray[i] = new Cylinder();
            }

            cylinderArray[0].mCylinderPosition = new Vector3(0, (float)(((0.4 * 22.5) / 100) + 0.2), 0);
            cylinderArray[1].mCylinderPosition = new Vector3(0.1f, (float)(((0.4 * -22.5) / 100) + 0.2), 0);
            cylinderArray[2].mCylinderPosition = new Vector3(-0.1f, (float)(((0.4 * -22.5) / 100) + 0.2), 0);
            cylinderArray[3].mCylinderPosition = new Vector3(0, 0.2f, 0);
            cylinderArray[4].mCylinderPosition = new Vector3(0, -0.2f, 0);
            cylinderArray[5].mCylinderPosition = new Vector3(0, -0.2f, 0);

            for (int i = 0; i < 3; i++)
            {
                cylinderArray[i].mCylinderRotation = new Vector3((float)(Math.PI / 2), 0, 0);
                cylinderArray[i].mCylinderRadius = (float)((0.4 * 7.5) / 100);
                cylinderArray[i].mCylinderScale = new Vector3(1, 6.5f, 1);
            }

            cylinderArray[3].mCylinderRotation = new Vector3((float)(Math.PI / 2), (float)(Math.PI / 2), 0);
            cylinderArray[4].mCylinderRotation = new Vector3((float)(-Math.PI / 3), (float)(-Math.PI / 4), 0);
            cylinderArray[5].mCylinderRotation = new Vector3((float)(Math.PI / 2), (float)(Math.PI / 4), 0);

            cylinderArray[3].mCylinderRadius = (float)((0.4 * 15) / 100);
            cylinderArray[4].mCylinderRadius = (float)((0.4 * 15) / 100);
            cylinderArray[5].mCylinderRadius = (float)((0.4 * 10) / 100);

            cylinderArray[3].mCylinderScale = new Vector3(1, 3.25f, 1);
            cylinderArray[4].mCylinderScale = new Vector3(1, 6, 1);
            cylinderArray[5].mCylinderScale = new Vector3(1, 7, 1);

            #region Texturing

            /*
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
            */

            #endregion

            #endregion

            #region Sphere

            sphereList.Add(CreateSphereItem());

            #endregion

            //This variable is used to select a ball to follow with the follow camera
            randomSphere = 0;

            //This variable is used to switche between integration methods
            simulationBool = true;

            //these variables are used to control the camera
            mWorld2 = mWorld;
            mouse = new Vector3(0, 0, 0);

            //These variables are used to animation speed
            mTimer = new Timer();
            mTimer.Start();

            #endregion
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            //This if statement blocks out camera movements
            if (camera == Camera.User)
            {
                //If W is pressed
                if (e.KeyChar == 'w' || e.KeyChar == 'W')
                {
                    //Translates the camera forward
                    mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, 0.05f);
                    int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView, true, ref mView);
                    int uView2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView2, true, ref mView);
                }

                //If A is pressed
                if (e.KeyChar == 'a' || e.KeyChar == 'A')
                {
                    //Translates the camera right
                    mView = mView * Matrix4.CreateRotationY(-0.025f);
                    int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView, true, ref mView);
                    int uView2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView2, true, ref mView);
                }

                //If S is pressed
                if (e.KeyChar == 's' || e.KeyChar == 'S')
                {
                    //Translates the camera back
                    mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, -0.05f);
                    int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView, true, ref mView);
                    int uView2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView2, true, ref mView);
                }

                //If D is pressed
                if (e.KeyChar == 'd' || e.KeyChar == 'D')
                {
                    //Translates the camera left
                    mView = mView * Matrix4.CreateRotationY(0.025f);
                    int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView, true, ref mView);
                    int uView2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uView");
                    GL.UniformMatrix4(uView2, true, ref mView);
                }

                //If Z is pressed
                if (e.KeyChar == 'z' || e.KeyChar == 'Z')
                {
                    //Spins the world left
                    Vector3 t = mWorld.ExtractTranslation();
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                    mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
                }

                //If C is pressed
                if (e.KeyChar == 'c' || e.KeyChar == 'C')
                {
                    //Spins the world right
                    Vector3 t = mWorld.ExtractTranslation();
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                    mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(0.025f) * translation;
                }

                //If R is pressed
                if (e.KeyChar == 'r' || e.KeyChar == 'R')
                {
                    //Spins the world forwards
                    Vector3 t = mWorld.ExtractTranslation();
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                    mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationX(-0.025f) * translation;
                }

                //If F is pressed
                if (e.KeyChar == 'f' || e.KeyChar == 'F')
                {
                    //Spins the world backwards
                    Vector3 t = mWorld.ExtractTranslation();
                    Matrix4 translation = Matrix4.CreateTranslation(t);
                    Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                    mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationX(0.025f) * translation;
                }
            }

            //If Q is pressed
            if (e.KeyChar == 'q' || e.KeyChar == 'Q')
            {
                //Increases the spheres velocity in the Y axis
                for (int i = 0; i < sphereList.Count; i++)
                {
                    sphereList[i].mSphereVelocity.Y = sphereList[i].mSphereVelocity.Y + 10;
                }
            }

            //If Q is pressed
            if (e.KeyChar == 'e' || e.KeyChar == 'E')
            {
                //This switches the integration method
                if (simulationBool)
                {
                    simulationBool = false;
                }
                else
                {
                    simulationBool = true;
                }
            }

            //If 1 is pressed
            if (e.KeyChar == '1')
            {
                //Selects static camera
                mWorld = mWorld2;
                camera = Camera.Static;
            }
            else
            {
                //If 2 is pressed
                if (e.KeyChar == '2')
                {
                    //Selects user camera
                    mWorld = mWorld2;
                    camera = Camera.User;
                }
                else
                {
                    //If 3 is presed
                    if (e.KeyChar == '3')
                    {
                        //Selects mouse camera
                        mWorld = mWorld2;
                        camera = Camera.Mouse;
                    }
                    else
                    {
                        //If 4 is pressed
                        if (e.KeyChar == '4')
                        {
                            //Selects moving camera
                            mWorld = mWorld2;
                            camera = Camera.Moving;
                        }
                        else
                        {
                            //If 5 is pressed
                            if (e.KeyChar == '5')
                            {
                                //Select follow camera
                                mWorld = mWorld2;
                                randomSphere = random.Next(0, (sphereList.Count));
                                camera = Camera.Follow;
                            }
                        }
                    }
                }
            }

            //If mark is pressed
            if (e.KeyChar == '!')
            {
                //Exit simulation
                OnUnload(e);

                Exit();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(this.ClientRectangle);

            //If shader is not null
            if (mShader != null && mShader2 != null)
            {
                //Resize simulation
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            #region Camera

            //If camera is moving
            if (camera == Camera.Moving)
            {
                //Splin camera
                Vector3 t = mWorld.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWorld = mWorld * inverseTranslation * Matrix4.CreateRotationY(-0.025f) * translation;
            }
            else
            {
                //If camera is mouse
                if (camera == Camera.Mouse)
                {
                    //Previous mouse is mouse
                    Vector3 previousMouse = mouse;
                    mouse = new Vector3((Mouse.X - 400), (Mouse.Y - 300), Mouse.Wheel);
                    float scale = mouse.Z;

                    //Sets scale rate relative to the zoom
                    if (scale < 1)
                    {
                        if (scale == 0)
                        {
                            scale = -1;
                        }

                        scale = -1 / scale;
                    }
                    
                    if (scale > 1)
                    {
                        if (scale > 5)
                        {
                            scale = scale / 2;
                        }

                        scale = scale / 2;
                    }

                    //Translated the world view
                    mWorld = mWorld * Matrix4.CreateTranslation(-((previousMouse.X - mouse.X) / (400 / scale)), ((previousMouse.Y - mouse.Y) / (400 / scale)), ((previousMouse.Z - mouse.Z) / 4));
                }
                else
                {
                    //If the camera is follow
                    if (camera == Camera.Follow)
                    {
                        //Follow a random sphere
                        mWorld.Row3 = new Vector4(-sphereList[randomSphere].mSpherePosition.X, -(sphereList[randomSphere].mSpherePosition.Y - 1.6f), -(sphereList[randomSphere].mSpherePosition.Z + 1), 1);
                    }
                }
            }
            #endregion

            #region Time

            //Increaces a float based on elapsed time
            float timestep = mTimer.GetElapsedSeconds();

            //Used to spawn spheres
            ellapsedTime = ellapsedTime + timestep;

            #endregion

            #region AddSphere

            //If ellapsed time is greater than an arbitary value
            if (ellapsedTime > randomEllapsedTime)
            {
                //Spawn new sphere
                sphereList.Add(CreateSphereItem());

                //Resets the ellapsed time
                ellapsedTime = 0;
                randomEllapsedTime = random.Next(0, 1001);
                randomEllapsedTime = randomEllapsedTime / 1000;
            }

            #endregion

            //Checks every sphere
            for (int i = 0; i < sphereList.Count; i++)
            {
                //Stores state of update
                bool collision = false;
                Vector3 mPreviousSpherePosition = sphereList[i].mSpherePosition;

                #region Velocity1

                if (simulationBool)
                {
                    //Increases velocity based on ellapsed time
                    sphereList[i].mSphereVelocity = sphereList[i].mSphereVelocity + (accelerationDueToGravity * timestep);
                }

                #endregion

                #region Position

                //Increases position based on ellapsed time
                sphereList[i].mSpherePosition = sphereList[i].mSpherePosition + (sphereList[i].mSphereVelocity * timestep);

                #endregion

                #region WallCollide

                
                if (sphereList[i].mSpherePosition.X + sphereList[i].mSphereRadius >= 0.2)
                {
                    //Sets state to true
                    collision = true;

                    //If the collision occurs over the portal
                    if (sphereList[i].mSpherePosition.Y - sphereList[i].mSphereRadius >= 0.4)
                    {
                        float temporaryVelocity = sphereList[i].mSphereVelocity.X;

                        //Swaps velocities
                        sphereList[i].mSphereVelocity.X = -sphereList[i].mSphereVelocity.Y;
                        sphereList[i].mSphereVelocity.Y = temporaryVelocity;

                        //Moves sphere to portal exit
                        sphereList[i].mSpherePosition.X = -sphereList[i].mSpherePosition.Y + 0.6f;

                        if (sphereList[i].mSpherePosition.X > 0)
                        {
                            sphereList[i].mSpherePosition.X = sphereList[i].mSpherePosition.X - sphereList[i].mSphereRadius;
                        }
                        else
                        {
                            if (sphereList[i].mSpherePosition.X < 0)
                            {
                                sphereList[i].mSpherePosition.X = sphereList[i].mSpherePosition.X + sphereList[i].mSphereRadius;
                            }
                        }

                        sphereList[i].mSpherePosition.Y = -0.8f + sphereList[i].mSphereRadius;
                    }
                    else
                    {
                        //Collision responce
                        sphereList[i].mSpherePosition = mPreviousSpherePosition;

                        Vector3 normal = new Vector3(-1, 0, 0);
                        sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                    }
                }

                //This checks for collisions with the right hand wall
                if (!collision && sphereList[i].mSpherePosition.X - sphereList[i].mSphereRadius <= -0.2)
                {
                    //Sets state to true
                    //Collision responce
                    collision = true;
                    sphereList[i].mSpherePosition = mPreviousSpherePosition;

                    Vector3 normal = new Vector3(-1, 0, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                }

                //This checks for collisions with the top wall
                if (!collision && sphereList[i].mSpherePosition.Y + sphereList[i].mSphereRadius >= 0.8)
                {
                    //Sets state to true
                    //Collision responce
                    collision = true;
                    sphereList[i].mSpherePosition = mPreviousSpherePosition;

                    Vector3 normal = new Vector3(0, -1, 0);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                }
                else
                {
                    //If the collision occurs over the portal
                    if (!collision && sphereList[i].mSpherePosition.Y - sphereList[i].mSphereRadius <= -0.8)
                    {
                        //Sets state to true
                        collision = true;

                        float temporaryVelocity = sphereList[i].mSphereVelocity.X;

                        //Swaps velocities
                        sphereList[i].mSphereVelocity.X = sphereList[i].mSphereVelocity.Y;
                        sphereList[i].mSphereVelocity.Y = temporaryVelocity;

                        //Moves sphere to portal exit
                        sphereList[i].mSpherePosition.Y = 0.6f - sphereList[i].mSpherePosition.X;

                        if (sphereList[i].mSpherePosition.Y > 0)
                        {
                            sphereList[i].mSpherePosition.Y = sphereList[i].mSpherePosition.Y - sphereList[i].mSphereRadius;
                        }
                        else
                        {
                            if (sphereList[i].mSpherePosition.Y < 0)
                            {
                                sphereList[i].mSpherePosition.Y = sphereList[i].mSpherePosition.Y + sphereList[i].mSphereRadius;
                            }
                        }

                        sphereList[i].mSpherePosition.X = 0.2f - sphereList[i].mSphereRadius;
                    }
                }

                //This checks for collisions with the front and back wall
                if (!collision && (sphereList[i].mSpherePosition.Z + sphereList[i].mSphereRadius >= 0.2 || sphereList[i].mSpherePosition.Z - sphereList[i].mSphereRadius <= -0.2))
                {
                    //Sets state to true
                    //Collision responce
                    collision = true;
                    sphereList[i].mSpherePosition = mPreviousSpherePosition;

                    Vector3 normal = new Vector3(0, 0, -1);
                    sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                }

                #endregion

                #region SphereOfDoomCollide

                //If no previous collision has occured
                if (!collision)
                {
                    //Variables to be used to create a flashlight
                    int uLightPositionLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].Position");
                    int uAmbientLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].AmbientLight");
                    int uDiffuseLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].DiffuseLight");
                    int uSpecularLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].SpecularLight");

                    //The distance between the current sphere and the sphere of doom plus the combined radius of the two spheres
                    double sphereDistance = Math.Sqrt(Math.Pow((sphereList[i].mSpherePosition.X - sphereOfDoom.mSpherePosition.X), 2) + Math.Pow((sphereList[i].mSpherePosition.Y - sphereOfDoom.mSpherePosition.Y), 2) + Math.Pow((sphereList[i].mSpherePosition.Z - sphereOfDoom.mSpherePosition.Z), 2));
                    double sphereRadius = sphereList[i].mSphereRadius + sphereOfDoom.mSphereRadius;

                    //If the distance is less than the combined radius
                    if (sphereDistance <= sphereRadius)
                    {
                        //Sets state to true
                        //Collision responce
                        collision = true;

                        //Sets the flashlight to be on
                        Vector4 lightPosition2 = Vector4.Transform(new Vector4(sphereList[i].mSpherePosition, 1), mView);
                        GL.Uniform4(uLightPositionLocation4, lightPosition2);

                        Vector3 colour4 = new Vector3(1, 0, 0);
                        GL.Uniform3(uAmbientLightLocation4, colour4);
                        GL.Uniform3(uDiffuseLightLocation4, colour4);
                        GL.Uniform3(uSpecularLightLocation4, colour4);

                        //This translates the sphere to the correct distance away from the sphere of doom
                        if (sphereList[i].mSphereRadius - (sphereRadius - sphereDistance) > 0)
                        {
                            if (sphereList[i].mSpherePosition.X > 0)
                            {
                                sphereList[i].mSpherePosition.X = sphereList[i].mSpherePosition.X + (float)(sphereRadius - sphereDistance);
                            }
                            else
                            {
                                if (sphereList[i].mSpherePosition.X < 0)
                                {
                                    sphereList[i].mSpherePosition.X = sphereList[i].mSpherePosition.X - (float)(sphereRadius - sphereDistance);
                                }
                            }

                            if (sphereList[i].mSpherePosition.Y > 0)
                            {
                                sphereList[i].mSpherePosition.Y = sphereList[i].mSpherePosition.Y + (float)(sphereRadius - sphereDistance);
                            }
                            else
                            {
                                if (sphereList[i].mSpherePosition.Y < 0)
                                {
                                    sphereList[i].mSpherePosition.Y = sphereList[i].mSpherePosition.Y - (float)(sphereRadius - sphereDistance);
                                }
                            }

                            if (sphereList[i].mSpherePosition.Z > 0)
                            {
                                sphereList[i].mSpherePosition.Z = sphereList[i].mSpherePosition.Z + (float)(sphereRadius - sphereDistance);
                            }
                            else
                            {
                                if (sphereList[i].mSpherePosition.Z < 0)
                                {
                                    sphereList[i].mSpherePosition.Z = sphereList[i].mSpherePosition.Z - (float)(sphereRadius - sphereDistance);
                                }
                            }

                            //This reduces the spheres radius by the amount the two spheres are intersecting
                            sphereList[i].mSphereRadius = sphereList[i].mSphereRadius - (float)(sphereRadius - sphereDistance);

                            //This ensures the new mass is correct
                            if (sphereList[i].mSphereBool)
                            {
                                sphereList[i].mSphereMass = (float)(0.0012 * ((4 / 3) * (Math.PI * Math.Pow(((sphereList[i].mSphereRadius * 100) / 0.4), 3))));
                            }
                            else
                            {
                                sphereList[i].mSphereMass = (float)(0.0014 * ((4 / 3) * (Math.PI * Math.Pow(((sphereList[i].mSphereRadius * 100) / 0.4), 3))));
                            }
                        }
                        else
                        {
                            //This checks to see if the currently observed sphere is the one being deleted
                            if (camera == Camera.Follow)
                            {
                                if (randomSphere == i)
                                {
                                    randomSphere = random.Next(0, (sphereList.Count - 1));
                                }
                                else
                                {
                                    if (randomSphere > i)
                                    {
                                        randomSphere = randomSphere - 1;
                                    }
                                }
                            }

                            //This moves all the spheres down the sphere list
                            for (int j = i; j < sphereList.Count - 1; j++)
                            {
                                sphereList[j] = sphereList[j + 1];
                            }

                            //This removes the last sphere from the sphere list
                            sphereList.RemoveAt(sphereList.Count - 1);

                            i--;

                            continue;
                        }
                    }
                }

                #endregion

                #region CylinderCollide

                //If no previous collision has occured
                if (!collision)
                {
                    //These are the variables used to calculate the cylinder collisions
                    Vector3 position1;
                    Vector3 position2;
                    Vector3 dot1;
                    Vector3 dot2;
                    Vector3 A;
                    Vector3 result;
                    Vector3 B;
                    float cylinderRadius;

                    for (int j = 0; j < 3; j++)
                    {
                        //This calculates the distance between the cylinder and the sphere for any point along the cylinder
                        position1 = cylinderArray[j].mCylinderPosition;
                        position2 = position1 + new Vector3(0, 0, 1);
                        dot1 = new Vector3(sphereList[i].mSpherePosition - position2);
                        dot2 = (position1 - position2).Normalized();
                        A = ((dot1.X * dot2.X) + (dot1.Y * dot2.Y) + (dot1.Z * dot2.Z)) * dot2;
                        result = position2 + A - sphereList[i].mSpherePosition;
                        B = position2 + A;
                        cylinderRadius = sphereList[i].mSphereRadius + cylinderArray[j].mCylinderRadius;

                        //If the distance between the cylinder and the sphere is less than their combined radius
                        if (result.Length <= cylinderRadius)
                        {
                            //Sets state to true
                            //Collision responce
                            collision = true;
                            sphereList[i].mSpherePosition = mPreviousSpherePosition;

                            Vector3 normal = (sphereList[i].mSpherePosition - B).Normalized();
                            sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;

                            break;
                        }
                    }

                    //If no previous collision has occured
                    if (!collision)
                    {
                        //This calculates the distance between the cylinder and the sphere for any point along the cylinder
                        position1 = cylinderArray[3].mCylinderPosition;
                        position2 = position1 + new Vector3(1, 0, 0);
                        dot1 = new Vector3(sphereList[i].mSpherePosition - position2);
                        dot2 = (position1 - position2).Normalized();
                        A = ((dot1.X * dot2.X) + (dot1.Y * dot2.Y) + (dot1.Z * dot2.Z)) * dot2;
                        result = position2 + A - sphereList[i].mSpherePosition;
                        B = position2 + A;
                        cylinderRadius = sphereList[i].mSphereRadius + cylinderArray[3].mCylinderRadius;

                        //If the distance between the cylinder and the sphere is less than their combined radius
                        if (result.Length <= cylinderRadius)
                        {
                            //Sets state to true
                            //Collision responce
                            collision = true;
                            sphereList[i].mSpherePosition = mPreviousSpherePosition;

                            Vector3 normal = (sphereList[i].mSpherePosition - B).Normalized();
                            sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                        }
                    }

                    //If no previous collision has occured
                    if (!collision)
                    {
                        //This calculates the distance between the cylinder and the sphere for any point along the cylinder
                        position1 = cylinderArray[4].mCylinderPosition;
                        position2 = position1 + new Vector3(3, 2, -3);
                        dot1 = new Vector3(sphereList[i].mSpherePosition - position2);
                        dot2 = (position1 - position2).Normalized();
                        A = ((dot1.X * dot2.X) + (dot1.Y * dot2.Y) + (dot1.Z * dot2.Z)) * dot2;
                        result = position2 + A - sphereList[i].mSpherePosition;
                        B = position2 + A;
                        cylinderRadius = sphereList[i].mSphereRadius + cylinderArray[4].mCylinderRadius;

                        //If the distance between the cylinder and the sphere is less than their combined radius
                        if (result.Length <= cylinderRadius)
                        {
                            //Sets state to true
                            //Collision responce
                            collision = true;
                            sphereList[i].mSpherePosition = mPreviousSpherePosition;

                            Vector3 normal = (sphereList[i].mSpherePosition - B).Normalized();
                            sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                        }
                    }

                    //If no previous collision has occured
                    if (!collision)
                    {
                        //This calculates the distance between the cylinder and the sphere for any point along the cylinder
                        position1 = cylinderArray[5].mCylinderPosition;
                        position2 = position1 + new Vector3(1, 0, 1);
                        dot1 = new Vector3(sphereList[i].mSpherePosition - position2);
                        dot2 = (position1 - position2).Normalized();
                        A = ((dot1.X * dot2.X) + (dot1.Y * dot2.Y) + (dot1.Z * dot2.Z)) * dot2;
                        result = position2 + A - sphereList[i].mSpherePosition;
                        B = position2 + A;
                        cylinderRadius = sphereList[i].mSphereRadius + cylinderArray[5].mCylinderRadius;

                        //If the distance between the cylinder and the sphere is less than their combined radius
                        if (result.Length <= cylinderRadius)
                        {
                            //Sets state to true
                            //Collision responce
                            collision = true;
                            sphereList[i].mSpherePosition = mPreviousSpherePosition;

                            Vector3 normal = (sphereList[i].mSpherePosition - B).Normalized();
                            sphereList[i].mSphereVelocity = (sphereList[i].mSphereVelocity - 2 * Vector3.Dot(normal, sphereList[i].mSphereVelocity) * normal) * coefficientOfRestitution;
                        }
                    }
                }
                #endregion

                #region SphereCollide

                //If no previous collision has occured
                if (!collision)
                {
                    //For all the spheres after this one
                    for (int j = i + 1; j < sphereList.Count; j++)
                    {
                        //If the distance between the sphere and the sphere is less than their combined radius
                        if (Math.Sqrt(Math.Pow((sphereList[i].mSpherePosition.X - sphereList[j].mSpherePosition.X), 2) + Math.Pow((sphereList[i].mSpherePosition.Y - sphereList[j].mSpherePosition.Y), 2) + Math.Pow((sphereList[i].mSpherePosition.Z - sphereList[j].mSpherePosition.Z), 2)) <= (sphereList[i].mSphereRadius + sphereList[j].mSphereRadius))
                        {
                            //Collision responce
                            sphereList[i].mSpherePosition = mPreviousSpherePosition;

                            Vector3 temporaryVelocity = sphereList[i].mSphereVelocity;

                            sphereList[i].mSphereVelocity = (Vector3.Multiply(sphereList[i].mSphereVelocity, ((sphereList[i].mSphereMass - sphereList[j].mSphereMass) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass))) + Vector3.Multiply(sphereList[j].mSphereVelocity, ((2 * sphereList[j].mSphereMass) / (sphereList[i].mSphereMass + sphereList[j].mSphereMass)))) * coefficientOfRestitution;
                            sphereList[j].mSphereVelocity = (Vector3.Multiply(sphereList[j].mSphereVelocity, ((sphereList[j].mSphereMass - sphereList[i].mSphereMass) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass))) + Vector3.Multiply(temporaryVelocity, ((2 * sphereList[i].mSphereMass) / (sphereList[j].mSphereMass + sphereList[i].mSphereMass)))) * coefficientOfRestitution;

                            break;
                        }
                    }
                }

                #endregion

                #region Velocity2

                if (!simulationBool)
                {
                    //Increases velocity based on ellapsed time
                    sphereList[i].mSphereVelocity = sphereList[i].mSphereVelocity + (accelerationDueToGravity * timestep);
                }

                #endregion
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            #region ShaderVariables

            //Variables to be used to create the lighting
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uModel2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uModel");
            int uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            int uAmbientReflectivityLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uMaterial.AmbientReflectivity");
            int uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            int uDiffuseReflectivityLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            int uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            int uSpecularReflectivityLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uMaterial.SpecularReflectivity");
            int uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");
            int uShininessLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uMaterial.Shininess");
            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");
            int uColourLocation2 = GL.GetUniformLocation(mShader2.ShaderProgramID, "uColour");

            #endregion

            #region World



            #endregion

            #region Box

            //This culls the front face of the box
            GL.Enable(EnableCap.CullFace);

            //GL.ActiveShaderProgram(mShader.ShaderProgramID, mShader.ShaderProgramID);

            //This sets the colour to black
            GL.Uniform4(uColourLocation, Color4.Black);
            GL.Uniform4(uColourLocation2, Color4.Transparent);

            #region EmitterBox

            //This is where the translation from on load is applied
            Matrix4 m1 = mEmitterBoxModel * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m1);

            //This is the ambient colour of the item
            Vector3 emitterBoxAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, emitterBoxAmbientReflectivity);

            //This is what colour light is reflected evenly from the item
            Vector3 emitterBoxDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, emitterBoxDiffuseReflectivity);

            //This is what colour light is reflected from the item
            Vector3 emitterBoxSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, emitterBoxSpecularReflectivity);

            //This is how much light is reflected
            float emitterBoxShininess = 76.8f;
            GL.Uniform1(uShininessLocation, emitterBoxShininess);

            //This binds the vertex array and draws the item
            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mEmitterBoxModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region GridBox1

            //This is where the translation from on load is applied
            Matrix4 m2 = mGridBox1Model * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);

            //This is the ambient colour of the item
            Vector3 GridBox1AmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, GridBox1AmbientReflectivity);

            //This is what colour light is reflected evenly from the item
            Vector3 GridBox1DiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, GridBox1DiffuseReflectivity);

            //This is what colour light is reflected from the item
            Vector3 GridBox1SpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, GridBox1SpecularReflectivity);

            //This is how much light is reflected
            float GridBox1Shininess = 76.8f;
            GL.Uniform1(uShininessLocation, GridBox1Shininess);

            //This binds the vertex array and draws the item
            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mGridBox1ModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region GridBox2

            //This is where the translation from on load is applied
            Matrix4 m3 = mGridBox2Model * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m3);

            //This is the ambient colour of the item
            Vector3 GridBox2AmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, GridBox2AmbientReflectivity);

            //This is what colour light is reflected evenly from the item
            Vector3 GridBox2DiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, GridBox2DiffuseReflectivity);

            //This is what colour light is reflected from the item
            Vector3 GridBox2SpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, GridBox2SpecularReflectivity);

            //This is how much light is reflected
            float GridBox2Shininess = 76.8f;
            GL.Uniform1(uShininessLocation, GridBox2Shininess);

            //This binds the vertex array and draws the item
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, mGridBox2ModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            #region SphereOfDoomBox

            //This is where the translation from on load is applied
            Matrix4 m4 = mSphereOfDoomBoxModel * mWorld;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m4);

            //This is the ambient colour of the item
            Vector3 SphereOfDoomBoxAmbientReflectivity = new Vector3(0.2125f, 0.1275f, 0.054f);
            GL.Uniform3(uAmbientReflectivityLocation, SphereOfDoomBoxAmbientReflectivity);

            //This is what colour light is reflected evenly from the item
            Vector3 SphereOfDoomBoxDiffuseReflectivity = new Vector3(0.714f, 0.4284f, 0.18144f);
            GL.Uniform3(uDiffuseReflectivityLocation, SphereOfDoomBoxDiffuseReflectivity);

            //This is what colour light is reflected from the item
            Vector3 SphereOfDoomBoxSpecularReflectivity = new Vector3(0.393548f, 0.271906f, 0.166721f);
            GL.Uniform3(uSpecularReflectivityLocation, SphereOfDoomBoxSpecularReflectivity);

            //This is how much light is reflected
            float SphereOfDoomBoxShininess = 76.8f;
            GL.Uniform1(uShininessLocation, SphereOfDoomBoxShininess);

            //This binds the vertex array and draws the item
            GL.BindVertexArray(mVAO_IDs[4]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereOfDoomBoxModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            #endregion

            //This stops the culling of the front face of the box
            GL.Disable(EnableCap.CullFace);

            #endregion

            #region Contents

            //GL.ActiveShaderProgram(mShader2.ShaderProgramID, mShader2.ShaderProgramID);

            #region Cylinder

            //This sets the colour to black
            GL.Uniform4(uColourLocation, Color4.Black);
            GL.Uniform4(uColourLocation2, Color4.Black);

            //For all the cylinders in the array
            for (int i = 0; i < cylinderArray.Length; i++)
            {
                //This is where the translation from on load is applied
                Matrix4 m5 = Matrix4.CreateScale(cylinderArray[i].mCylinderScale) * Matrix4.CreateRotationX(cylinderArray[i].mCylinderRotation.X) * Matrix4.CreateRotationY(cylinderArray[i].mCylinderRotation.Y) * Matrix4.CreateScale(cylinderArray[i].mCylinderRadius) * Matrix4.CreateTranslation(cylinderArray[i].mCylinderPosition) * mWorld;
                uModel = GL.GetUniformLocation(mShader2.ShaderProgramID, "uModel");
                GL.UniformMatrix4(uModel2, true, ref m5);

                //This is the ambient colour of the item
                Vector3 CylinderAmbientReflectivity = new Vector3(0.1f, 0.1f, 0.1f);
                GL.Uniform3(uAmbientReflectivityLocation2, CylinderAmbientReflectivity);

                //This is what colour light is reflected evenly from the item
                Vector3 CylinderDiffuseReflectivity = new Vector3(0, 0, 0);
                GL.Uniform3(uDiffuseReflectivityLocation2, CylinderDiffuseReflectivity);

                //This is what colour light is reflected from the item
                Vector3 CylinderSpecularReflectivity = new Vector3(0, 0, 0);
                GL.Uniform3(uSpecularReflectivityLocation2, CylinderSpecularReflectivity);

                //This is how much light is reflected
                float CylinderShininess = 0;
                GL.Uniform1(uShininessLocation2, CylinderShininess);

                //This binds the vertex array and draws the item
                GL.BindVertexArray(mVAO_IDs[6]);
                GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            #endregion

            #region SphereOfDoom

            //This sets the colour to black
            GL.Uniform4(uColourLocation, Color4.DodgerBlue);
            GL.Uniform4(uColourLocation2, Color4.DodgerBlue);

            //This is where the translation from on load is applied
            Matrix4 m6 = Matrix4.CreateScale(sphereOfDoom.mSphereRadius) * Matrix4.CreateTranslation(sphereOfDoom.mSpherePosition) * mWorld;
            uModel = GL.GetUniformLocation(mShader2.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel2, true, ref m6);

            //This is the ambient colour of the item
            Vector3 SphereOfDoomAmbientReflectivity = new Vector3(0.1f, 0.1f, 0.1f);
            GL.Uniform3(uAmbientReflectivityLocation2, SphereOfDoomAmbientReflectivity);

            //This is what colour light is reflected evenly from the item
            Vector3 SphereOfDoomDiffuseReflectivity = new Vector3(0, 0, 0);
            GL.Uniform3(uDiffuseReflectivityLocation2, SphereOfDoomDiffuseReflectivity);

            //This is what colour light is reflected from the item
            Vector3 SphereOfDoomSpecularReflectivity = new Vector3(0, 0, 0);
            GL.Uniform3(uSpecularReflectivityLocation2, SphereOfDoomSpecularReflectivity);

            //This is how much light is reflected
            float SphereOfDoomShininess = 0;
            GL.Uniform1(uShininessLocation2, SphereOfDoomShininess);

            //This binds the vertex array and draws the item
            GL.BindVertexArray(mVAO_IDs[7]);
            GL.DrawElements(PrimitiveType.Triangles, mSphereOfDoomModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            
            //This resets the flashlight back to black
            int uLightPositionLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].Position");
            int uAmbientLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].AmbientLight");
            int uDiffuseLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].DiffuseLight");
            int uSpecularLightLocation4 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[3].SpecularLight");

            Vector3 colour4 = new Vector3(0, 0, 0);
            GL.Uniform3(uAmbientLightLocation4, colour4);
            GL.Uniform3(uDiffuseLightLocation4, colour4);
            GL.Uniform3(uSpecularLightLocation4, colour4);

            #endregion

            #region Sphere

            //For all the spheres in the list
            for (int i = 0; i < sphereList.Count; i++)
            {
                //If it is a small sphere
                if (sphereList[i].mSphereBool)
                {
                    //Set the colour to green
                    GL.Uniform4(uColourLocation, Color4.ForestGreen);
                    GL.Uniform4(uColourLocation2, Color4.ForestGreen);

                    //This is what colour light is reflected evenly from the item
                    Vector3 SphereDiffuseReflectivity = new Vector3(0, 0, 0);
                    GL.Uniform3(uDiffuseReflectivityLocation2, SphereDiffuseReflectivity);

                    //This is what colour light is reflected from the item
                    Vector3 SphereSpecularReflectivity = new Vector3(0, 0, 0);
                    GL.Uniform3(uSpecularReflectivityLocation2, SphereSpecularReflectivity);

                    //This is how much light is reflected
                    float SphereShininess = 0;
                    GL.Uniform1(uShininessLocation2, SphereShininess);
                }
                else
                {
                    //Set the colour to purple
                    GL.Uniform4(uColourLocation, Color4.MediumPurple);
                    GL.Uniform4(uColourLocation2, Color4.MediumPurple);

                    //This is what colour light is reflected evenly from the item
                    Vector3 SphereDiffuseReflectivity = new Vector3(0.2f, 0.2f, 0.2f);
                    GL.Uniform3(uDiffuseReflectivityLocation2, SphereDiffuseReflectivity);

                    //This is what colour light is reflected from the item
                    Vector3 SphereSpecularReflectivity = new Vector3(0.2f, 0.2f, 0.2f);
                    GL.Uniform3(uSpecularReflectivityLocation2, SphereSpecularReflectivity);

                    //This is how much light is reflected
                    float SphereShininess = 2;
                    GL.Uniform1(uShininessLocation2, SphereShininess);
                }

                //This is where the translation from on load is applied
                Matrix4 m7 = Matrix4.CreateScale(sphereList[i].mSphereRadius) * Matrix4.CreateTranslation(sphereList[i].mSpherePosition) * mWorld;
                uModel = GL.GetUniformLocation(mShader2.ShaderProgramID, "uModel");
                GL.UniformMatrix4(uModel2, true, ref m7);

                //This is the ambient colour of the item
                Vector3 SphereAmbientReflectivity = new Vector3(0.1f, 0.1f, 0.1f);
                GL.Uniform3(uAmbientReflectivityLocation2, SphereAmbientReflectivity);

                //This binds the vertex array and draws the item
                GL.BindVertexArray(mVAO_IDs[7]);
                GL.DrawElements(PrimitiveType.Triangles, mSphereModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            #endregion

            #endregion

            //This swaps the current buffer with the most recently rendered buffer
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            //This frees the graphics memory up
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            //This unbindes the bufferes
            GL.BindVertexArray(0);

            //This deletes the buffers
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);

            //This deleted the shaders
            mShader.Delete();
            mShader2.Delete();
        }
    }
}