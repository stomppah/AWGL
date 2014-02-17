﻿using KAOS.Interfaces;
using KAOS.Shapes;
using KAOS.Utilities;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace KAOS.Managers
{
    public class BufferObjectManager
    {
        Dictionary<string, BufferObject> m_bufferStore = new Dictionary<string, BufferObject>();

        public void AddBufferObject(string name, IDrawableShape shape, int program)
        {
            BufferObject bufferObject = new BufferObject();
            //bufferObject.PositionData = new Vector3d[1];
            //bufferObject.NormalsData = new Vector3d[1];
            //VertexT2fN3fV3f[] vertexData;
            //uint[] indices;
            //BeginMode type;

            //shape.GetArraysforVBO(out type, out vertexData, out indices);

            bufferObject.PositionData = shape.Vertices;
            bufferObject.NormalsData = shape.Normals;
            bufferObject.IndicesData = shape.Indices;

            int bufferHandle;

            #region Get sizes of buffer stores
            int sizeOfPositionData = Vector3.SizeInBytes * bufferObject.PositionData.Length;
            int sizeOfNormalsData = Vector3.SizeInBytes * bufferObject.NormalsData.Length;
            //int sizeOfColorData = Marshal.SizeOf(new Color4()) * bufferObject.ColorData.Length;
            IntPtr bufferSize = new IntPtr (sizeOfPositionData + sizeOfNormalsData);
            IntPtr noOffset = new IntPtr(0);
            #endregion

            // Generate Vertex Buffer Object and bind it so it is current.
            GL.GenBuffers(1, out bufferHandle);         
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);

            #region Save pointers generated by OpenGL here so i dont forget.
            bufferObject.VboID = bufferHandle; 
            #endregion
            
            #region Send all data to the Vertex Buffer
            // Initialise storage space for the Vertex Buffer.
            GL.BufferData(BufferTarget.ArrayBuffer, bufferSize, IntPtr.Zero, BufferUsageHint.StaticDraw);
            // Send Position data.
            GL.BufferSubData<Vector3>(
                BufferTarget.ArrayBuffer, noOffset, new IntPtr(sizeOfPositionData), bufferObject.PositionData);
            // Send Normals data, offset by size of Position data.
            GL.BufferSubData<Vector3>(
                BufferTarget.ArrayBuffer, new IntPtr(sizeOfPositionData), new IntPtr(sizeOfNormalsData), bufferObject.NormalsData);
            
            GL.GenBuffers(1, out bufferHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferHandle);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * bufferObject.IndicesData.Length), bufferObject.IndicesData, BufferUsageHint.StaticDraw);

            bufferObject.IboID = bufferHandle;

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferObject.IboID);
            #endregion

            // GL3 allows us to store the vertex layout in a "vertex array object" (VAO).
            // This means we do not have to re-issue VertexAttribPointer calls
            // every time we try to use a different vertex layout - these calls are
            // stored in the VAO so we simply need to bind the correct VAO.

            // Generate Vertex Array Object and bind it so it is current.
            GL.GenVertexArrays(1, out bufferHandle);
            GL.BindVertexArray(bufferHandle);

            bufferObject.VaoID = bufferHandle;

            bufferHandle = GL.GetAttribLocation(program, "in_position");
            GL.EnableVertexAttribArray(bufferHandle); 
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject.VboID);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            GL.BindAttribLocation(program, bufferHandle, "in_position");

            bufferHandle = GL.GetAttribLocation(program, "in_normal");
            GL.EnableVertexAttribArray(bufferHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObject.VboID);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, sizeOfPositionData);
            GL.BindAttribLocation(program, bufferHandle, "in_normal");

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferObject.IboID);

            // IMPORTANT: vertex array needs unbinding here to avoid rendering incorrectly
            GL.BindVertexArray(0);

            m_bufferStore.Add(name, bufferObject);
        }

        public BufferObject GetBuffer(string name)
        {
            return m_bufferStore[name];
        }
    }
}
