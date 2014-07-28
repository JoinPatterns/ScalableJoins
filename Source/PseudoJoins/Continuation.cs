//------------------------------------------------------------------------------
// <copyright file="Continuation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Research.Joins {

  public delegate void Continuation();

  [global::System.AttributeUsage(
     AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class,
     Inherited = false, AllowMultiple = false)]
  public abstract class ContinuationAttribute : Attribute {

    public abstract void BeginInvoke(Continuation task);
    public abstract void Invoke(Continuation task);
    private static DefaultContinuationAttribute mDefaultContinuationAttribute = new DefaultContinuationAttribute();
    internal static ContinuationAttribute GetContinuationAttribute(Delegate d) {
      ContinuationAttribute continuationAttribute = System.Attribute.GetCustomAttribute((System.Reflection.MemberInfo)d.Method, typeof(ContinuationAttribute), false) as ContinuationAttribute;
      if (continuationAttribute == null) {
        System.Type declaringType = d.Method.DeclaringType;
        if (declaringType != null) {
          continuationAttribute = System.Attribute.GetCustomAttribute(declaringType, typeof(ContinuationAttribute), false) as ContinuationAttribute;
        }
        if (continuationAttribute == null)
          continuationAttribute = mDefaultContinuationAttribute;
      }
      return continuationAttribute;
    }
  }

  public class DefaultContinuationAttribute : ContinuationAttribute {
    public override void BeginInvoke(Continuation task) {
      new Thread(task.Invoke).Start();
    }
    public override void Invoke(Continuation task) {
      task();
    }
  }

}