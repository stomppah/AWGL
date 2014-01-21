﻿using OpenTK;
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
        private AWShaderManager shaderManager;

        int modelviewMatrixLocation,
            projectionMatrixLocation,
            buffer,
            vao,
            mv_location,
            proj_location;

        Matrix4 proj_matrix, modelviewMatrix;

        public ShaderTutorials()
            : base(800, 600, new GraphicsMode(), "", 0,
            DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            
        }

        private Vector3[] vertex_positions = new Vector3[]
        {
            new Vector3(0.25f,  0.25f, -0.25f),
            new Vector3(-0.25f, -0.25f, -0.25f),
            new Vector3( 0.25f, -0.25f, -0.25f),

            new Vector3( 0.25f, -0.25f, -0.25f),
            new Vector3( 0.25f,  0.25f, -0.25f),
            new Vector3(-0.25f,  0.25f, -0.25f),

            new Vector3( 0.25f, -0.25f, -0.25f),
            new Vector3( 0.25f, -0.25f,  0.25f),
            new Vector3( 0.25f,  0.25f, -0.25f),

            new Vector3( 0.25f, -0.25f,  0.25f),
            new Vector3( 0.25f,  0.25f,  0.25f),
            new Vector3(0.25f,  0.25f, -0.25f),

            new Vector3( 0.25f, -0.25f,  0.25f),
            new Vector3(-0.25f, -0.25f,  0.25f),
            new Vector3( 0.25f,  0.25f,  0.25f),

            new Vector3(-0.25f, -0.25f,  0.25f),
            new Vector3(-0.25f,  0.25f,  0.25f),
            new Vector3( 0.25f,  0.25f,  0.25f),

            new Vector3(-0.25f, -0.25f,  0.25f),
            new Vector3(-0.25f, -0.25f, -0.25f),
            new Vector3(-0.25f,  0.25f,  0.25f),

            new Vector3(-0.25f, -0.25f, -0.25f),
            new Vector3(-0.25f,  0.25f, -0.25f),
            new Vector3(-0.25f,  0.25f,  0.25f),

            new Vector3(-0.25f, -0.25f,  0.25f),
            new Vector3( 0.25f, -0.25f,  0.25f),
            new Vector3( 0.25f, -0.25f, -0.25f),

            new Vector3( 0.25f, -0.25f, -0.25f),
            new Vector3(-0.25f, -0.25f, -0.25f),
            new Vector3(-0.25f, -0.25f,  0.25f),

            new Vector3(-0.25f,  0.25f, -0.25f),
            new Vector3( 0.25f,  0.25f, -0.25f),
            new Vector3( 0.25f,  0.25f,  0.25f),

            new Vector3( 0.25f,  0.25f,  0.25f),
            new Vector3(-0.25f,  0.25f,  0.25f),
            new Vector3(0.25f,  0.25f, -0.25f)
        };

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VSync = VSyncMode.On;

            CreateShaders();

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out buffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer,
                         new IntPtr(vertex_positions.Length * Vector3.SizeInBytes),
                         vertex_positions,
                         BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);

            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.ClearColor(System.Drawing.Color.MidnightBlue);

            Title = AWUtils.PrintOpenGLInfo();
        }

        private void CreateShaders()
        {
            shaderManager = new AWShaderManager("opentk-vs", "opentk-fs");

            GL.UseProgram(shaderManager.ProgramHandle);

            // Set uniforms
            projectionMatrixLocation = GL.GetUniformLocation(shaderManager.ProgramHandle, "projection_matrix");
            modelviewMatrixLocation = GL.GetUniformLocation(shaderManager.ProgramHandle, "modelview_matrix");

            float aspectRatio = ClientSize.Width / (float)(ClientSize.Height);
            Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspectRatio, 1, 100, out proj_matrix);
            modelviewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

            GL.UniformMatrix4(projectionMatrixLocation, false, ref proj_matrix);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

            int attachedShaders;
            GL.GetProgram(shaderManager.ProgramHandle, GetProgramParameterName.AttachedShaders, out attachedShaders);
            Debug.WriteLine("/nAttached Shaders: " + attachedShaders);
        }

       protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[OpenTK.Input.Key.Escape])
                Exit();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            float[] green = { 0.0f, 0.25f, 0.0f, 1.0f };
            float one = 1.0f;

            GL.Viewport(0, 0, Width, Height);
            GL.ClearBuffer(ClearBuffer.Color, 0, green);
            GL.ClearBuffer(ClearBuffer.Depth, 0, ref one);

            GL.UseProgram(shaderManager.ProgramHandle);

            GL.UniformMatrix4(proj_location, 1, false, proj_matrix);

            int i;
            for (i = 0; i < 24; i++)
            {
                float f = (float)i + (float)currentTime * 0.3f;
                vmath::mat4 mv_matrix = vmath::translate(0.0f, 0.0f, -6.0f) *
                                        vmath::rotate((float)currentTime * 45.0f, 0.0f, 1.0f, 0.0f) *
                                        vmath::rotate((float)currentTime * 21.0f, 1.0f, 0.0f, 0.0f) *
                                        vmath::translate(sinf(2.1f * f) * 2.0f,
                                                         cosf(1.7f * f) * 2.0f,
                                                         sinf(1.3f * f) * cosf(1.5f * f) * 2.0f);
                GL.UniformMatrix4fv(mv_location, 1, GL_FALSE, mv_matrix);
                GL.DrawArrays(GL_TRIANGLES, 0, 36);
            }

            SwapBuffers();
        }

    }
}