using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab4
{
    public class Lab4_1Window : GameWindow
    {
        private Timer mTimer;
        private int[] mVertexArrayObjectIDArray = new int[2];
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;
        private Matrix4 mSquareMatrix, mSquareMatrix2;
        private Vector3 mCirclePosition, mPreviousCirclePosition, mCirclePosition2;
        private Vector3 mCircleVelocity;
        private float mCircleRadius, mCircleRadius2;

        public Lab4_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 4_1 Simple Animation and Collision Detection",
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
            GL.ClearColor(Color4.AliceBlue);

            mShader = new ShaderUtility(@"Lab4/Shaders/vLab4.vert", @"Lab4/Shaders/fLab4.frag");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.UseProgram(mShader.ShaderProgramID);

            float[] vertices = new float[] { 
                   -1f, -1f,
                   1f, -1f,
                   1f, 1f,
                   -1f, 1f
            };

            GL.GenVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.GenBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);

            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            vertices = new float[200];

            for (int i = 0; i < 100; ++i)
            {
                vertices[2 * i] = (float)Math.Cos(MathHelper.DegreesToRadians(i * 360.0 / 100));
                vertices[2 * i + 1] = (float)Math.Cos(MathHelper.DegreesToRadians(90.0 + i * 360.0 / 100));
            }

            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[1]);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            Matrix4 m = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4(uViewLocation, true, ref m);

            mSquareMatrix = Matrix4.CreateScale(3, 2, 1) * Matrix4.CreateRotationZ(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0);
            mSquareMatrix2 = Matrix4.CreateScale(1) * Matrix4.CreateRotationZ(0) * Matrix4.CreateTranslation(0, 0, 0);

            mCircleRadius = 0.1f;
            mCircleRadius2 = 0.2f;
            mCirclePosition = new Vector3(0, 0, 0);
            mCirclePosition2 = new Vector3(0.5f, 0.5f, 0);
            mPreviousCirclePosition = mCirclePosition;
            mCircleVelocity = new Vector3(1, 1, 0);

            base.OnLoad(e);

            mTimer = new Timer();
            mTimer.Start();
        }

        private void SetCamera()
        {
            float height = ClientRectangle.Height;
            float width = ClientRectangle.Width;
            if (mShader != null)
            {
                Matrix4 proj;
                if (height > width)
                {
                    if (width == 0)
                    {
                        width = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10, 10 * height / width, 0, 10);
                }
                else
                {
                    if (height == 0)
                    {
                        height = 1;
                    }
                    proj = Matrix4.CreateOrthographic(10 * width / height, 10, 0, 10);
                }
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                GL.UniformMatrix4(uProjectionLocation, true, ref proj);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            SetCamera();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float timestep = mTimer.GetElapsedSeconds();
            mCirclePosition = mCirclePosition + mCircleVelocity * timestep;

            Vector3 circleInSquareSpace = Vector3.Transform(mCirclePosition, mSquareMatrix.Inverted());

            if ((circleInSquareSpace.X + (mCircleRadius / mSquareMatrix.ExtractScale().X)) >= 1 || (circleInSquareSpace.X - (mCircleRadius / mSquareMatrix.ExtractScale().X)) <= -1)
            {
                Vector3 normal = Vector3.Transform(new Vector3(-1, 0, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;

                mCirclePosition = mPreviousCirclePosition;
            }

            if ((circleInSquareSpace.Y + (mCircleRadius / mSquareMatrix.ExtractScale().Y)) >= 1 || (circleInSquareSpace.Y - (mCircleRadius / mSquareMatrix.ExtractScale().Y)) <= -1)
            {
                Vector3 normal = Vector3.Transform(new Vector3(0, -1, 0), mSquareMatrix.ExtractRotation());
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;

                mCirclePosition = mPreviousCirclePosition;
            }

            if (Math.Sqrt(Math.Pow((mCirclePosition.X - mCirclePosition2.X), 2) + Math.Pow((mCirclePosition.Y - mCirclePosition2.Y), 2)) <= (mCircleRadius + mCircleRadius2))
            {
                Vector3 normal = (mCirclePosition - mCirclePosition2).Normalized();
                mCircleVelocity = mCircleVelocity - 2 * Vector3.Dot(normal, mCircleVelocity) * normal;

                mCirclePosition = mPreviousCirclePosition;
            }

            mPreviousCirclePosition = mCirclePosition;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelMatrixLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uColourLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uColour");

            GL.Uniform4(uColourLocation, Color4.DodgerBlue);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mSquareMatrix2);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            Matrix4 mCircleMatrix = Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mCircleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            Matrix4 mFixedCircleMatrix = Matrix4.CreateScale(mCircleRadius2) * Matrix4.CreateTranslation(mCirclePosition2);

            GL.UniformMatrix4(uModelMatrixLocation, true, ref mFixedCircleMatrix);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            GL.Uniform4(uColourLocation, Color4.Red);

            Matrix4 m = mSquareMatrix * mSquareMatrix.Inverted();

            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            m = mSquareMatrix2 * mSquareMatrix.Inverted();

            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[0]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 4);

            m = (Matrix4.CreateScale(mCircleRadius) * Matrix4.CreateTranslation(mCirclePosition)) * mSquareMatrix.Inverted();

            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            m = (Matrix4.CreateScale(mCircleRadius2) * Matrix4.CreateTranslation(mCirclePosition2)) * mSquareMatrix.Inverted();

            GL.UniformMatrix4(uModelMatrixLocation, true, ref m);
            GL.BindVertexArray(mVertexArrayObjectIDArray[1]);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 100);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(mVertexBufferObjectIDArray.Length, mVertexBufferObjectIDArray);
            GL.DeleteVertexArrays(mVertexArrayObjectIDArray.Length, mVertexArrayObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}