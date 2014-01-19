﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWGL
{
    public interface IGroupNode : ISceneNode, IEnumerable<ISceneNode>
    {
        void AddChild(ISceneNode child);
        void RemoveChild(ISceneNode child);
    }
}