﻿
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AWGL
{
    public class ShaderTutorials : GameWindow
    {

        int modelviewMatrixLocation,
            projectionMatrixLocation,
            vaoHandle,
            positionVboHandle,
            normalVboHandle,
            eboHandle;

        Vector3[] positionVboData = new Vector3[]{
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f), 
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f) };

        int[] indicesVboData = new int[]{
             // front face
                0, 1, 2, 2, 3, 0,
                // top face
                3, 2, 6, 6, 7, 3,
                // back face
                7, 6, 5, 5, 4, 7,
                // left face
                4, 0, 3, 3, 7, 4,
                // bottom face
                0, 1, 5, 5, 4, 0,
                // right face
                1, 5, 6, 6, 2, 1, };

        Matrix4 projectionMatrix, modelviewMatrix;

        AWShaderManager shaderManager;
        AWBufferManager bufferManager;

        public ShaderTutorials()
            : base(800, 600,
            new GraphicsMode(), "OpenGL 3 Example", 0,
            DisplayDevice.Default, 3, 0,
            GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        { }
        
        protected override void OnLoad (System.EventArgs e)
        {
            VSync = VSyncMode.On;

            CreateShaders();
            CreateVBOs();
            CreateVAOs();

            // Other state
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(System.Drawing.Color.MidnightBlue);
        }

        void CreateShaders()
        {
            shaderManager = new AWShaderManager("opentk-vs", "opentk-fs");

            GL.UseProgram(shaderManager.ProgramHandle);

            shaderManager.SetUniforms(
                out projectionMatrixLocation, out modelviewMatrixLocation,
                out projectionMatrix, out modelviewMatrix, ClientSize
            );
        }

        void CreateVBOs()
        {
            bufferManager = new AWBufferManager();

            bufferManager.SetupBuffer(
                out positionVboHandle, positionVboData, 
                BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);

            bufferManager.SetupBuffer(
                out normalVboHandle, positionVboData,
                BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw
                );

            bufferManager.SetupBuffer(
                out eboHandle, indicesVboData, 
                BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw
                );

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        void CreateVAOs()
        {
            // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
            // This means we do not have to re-issue VertexAttribPointer calls
            // every time we try to use a different vertex layout - these calls are
            // stored in the VAO so we simply need to bind the correct VAO.
            bufferManager.GenerateVaoBuffer(out vaoHandle);
            bufferManager.SetupVaoBuffer(
                positionVboHandle, shaderManager.ProgramHandle, 0, 3,"in_position",
                BufferTarget.ArrayBuffer, VertexAttribPointerType.Float
                );

            bufferManager.SetupVaoBuffer(
                normalVboHandle, shaderManager.ProgramHandle, 1, 3, "in_normal",
                BufferTarget.ArrayBuffer, VertexAttribPointerType.Float
                );

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);

            GL.BindVertexArray(0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
            Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

            if (Keyboard[OpenTK.Input.Key.Escape])
                Exit();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(
                PrimitiveType.Triangles, indicesVboData.Length,
                DrawElementsType.UnsignedInt, IntPtr.Zero
                );

            SwapBuffers();
        }


    }
}
