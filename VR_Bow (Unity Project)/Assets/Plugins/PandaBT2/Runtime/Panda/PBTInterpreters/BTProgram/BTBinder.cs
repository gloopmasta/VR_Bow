using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace PandaBT.Runtime
{
    public class BTBinder 
    {
        public object target;
        virtual public string name { get; }
        public MemberInfo memberInfo;
    }
}
