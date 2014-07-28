//------------------------------------------------------------------------------
// <copyright file="StackArray.cs" company="Microsoft">
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Joins {
  // a stack-allocated array
  internal interface IStackArray<A> {
    int Length { get; }
    A this [int a] { get; set;}
  }

  internal abstract class StackArray<A> {
    
    internal struct EmptyArray : IStackArray<A> {

      public int Length { get { return 0; } }

      public A this[int i] {
        get { throw new IndexOutOfRangeException(); }
        set { throw new IndexOutOfRangeException(); }
      }
    }

    internal struct SingletonArray : IStackArray<A> {
      private A mA;

      public int Length { get { return 1; } }

      public A this[int i] {
        get {
          // if (i != 0) throw new IndexOutOfRangeException();
          return mA;
        }
        set {
          //if (i != 0) throw new IndexOutOfRangeException();
          mA = value;
        }
      }
    }

    internal struct StackArray4 : IStackArray<A> {
      private A mA_0;
      private A mA_1;
      private A mA_2;
      private A mA_3;

      public int Length { get { return 4; } }

      //NB: we never throw IndexOutOfRange since thay may prevent inlining. 
      public A this[int i] {
        get {
          if (i == 0) return mA_0;
          if (i == 1) return mA_1;
          if (i == 2) return mA_2;
          return mA_3;
        }
        set {
          switch (i) {
            case 0: mA_0 = value; return;
            case 1: mA_1 = value; return;
            case 2: mA_2 = value; return;
            case 3: mA_3 = value; return;
            default: return;
          }
        }
      }

      public void Foo() {
        mA_0 = mA_1;
      }
    }


    internal struct StackArray8 : IStackArray<A>
    {
        private A mA_0;
        private A mA_1;
        private A mA_2;
        private A mA_3;
        private A mA_4;
        private A mA_5;
        private A mA_6;
        private A mA_7;

        public int Length { get { return 8; } }

        //NB: we never throw IndexOutOfRange since thay may prevent inlining. 
        public A this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return mA_0;
                    case 1: return mA_1;
                    case 2: return mA_2;
                    case 3: return mA_3;
                    case 4: return mA_4;
                    case 5: return mA_5;
                    case 6: return mA_6;
                    case 7: return mA_7;
                    default:
                        return default(A);
                }
            }
            set
            {
                switch (i)
                {
                    case 0: mA_0 = value; return;
                    case 1: mA_1 = value; return;
                    case 2: mA_2 = value; return;
                    case 3: mA_3 = value; return;
                    case 4: mA_4 = value; return;
                    case 5: mA_5 = value; return;
                    case 6: mA_6 = value; return;
                    case 7: mA_7 = value; return;
                    default: return;
                }
            }
        }
    }

    internal struct StackArray16 : IStackArray<A>
    {
        private A mA_0;
        private A mA_1;
        private A mA_2;
        private A mA_3;
        private A mA_4;
        private A mA_5;
        private A mA_6;
        private A mA_7;

        private A mA_8;
        private A mA_9;
        private A mA_10;
        private A mA_11;
        private A mA_12;
        private A mA_13;
        private A mA_14;
        private A mA_15;

       
        public int Length { get { return 16; } }

        //NB: we never throw IndexOutOfRange since thay may prevent inlining. 
        public A this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return mA_0;
                    case 1: return mA_1;
                    case 2: return mA_2;
                    case 3: return mA_3;
                    case 4: return mA_4;
                    case 5: return mA_5;
                    case 6: return mA_6;
                    case 7: return mA_7;

                    case 8: return mA_8;
                    case 9: return mA_9;
                    case 10: return mA_10;
                    case 11: return mA_11;
                    case 12: return mA_12;
                    case 13: return mA_13;
                    case 14: return mA_14;
                    case 15: return mA_15;
                    default:
                        return default(A);
                }
            }
            set
            {
                switch (i)
                {
                    case 0: mA_0 = value; return;
                    case 1: mA_1 = value; return;
                    case 2: mA_2 = value; return;
                    case 3: mA_3 = value; return;
                    case 4: mA_4 = value; return;
                    case 5: mA_5 = value; return;
                    case 6: mA_6 = value; return;
                    case 7: mA_7 = value; return;

                    case 8: mA_8 = value; return;
                    case 9: mA_9 = value; return;
                    case 10: mA_10 = value; return;
                    case 11: mA_11 = value; return;
                    case 12: mA_12 = value; return;
                    case 13: mA_13 = value; return;
                    case 14: mA_14 = value; return;
                    case 15: mA_15 = value; return;
                    default: return;
                }
            }
        }
    }

    internal struct StackArray32 : IStackArray<A> {
      private A mA_0;
      private A mA_1;
      private A mA_2;
      private A mA_3;
      private A mA_4;
      private A mA_5;
      private A mA_6;
      private A mA_7;

      private A mA_8;
      private A mA_9;
      private A mA_10;
      private A mA_11;
      private A mA_12;
      private A mA_13;
      private A mA_14;
      private A mA_15;
      
      private A mA_16;
      private A mA_17;
      private A mA_18;
      private A mA_19;
      private A mA_20;
      private A mA_21;
      private A mA_22;
      private A mA_23;
      
      private A mA_24;
      private A mA_25;
      private A mA_26;
      private A mA_27;
      private A mA_28;
      private A mA_29;
      private A mA_30;
      private A mA_31;

      public int Length { get { return 32; } }

      //NB: we never throw IndexOutOfRange since thay may prevent inlining. 
      public A this[int i] {
        get {
          switch (i) {
            case 0: return mA_0;
            case 1: return mA_1;
            case 2: return mA_2;
            case 3: return mA_3;
            case 4: return mA_4;
            case 5: return mA_5;
            case 6: return mA_6;
            case 7: return mA_7;

            case 8: return mA_8;
            case 9: return mA_9;
            case 10: return mA_10;
            case 11: return mA_11;
            case 12: return mA_12;
            case 13: return mA_13;
            case 14: return mA_14;
            case 15: return mA_15;

            case 16: return mA_16;
            case 17: return mA_17;
            case 18: return mA_18;
            case 19: return mA_19;
            case 20: return mA_20;
            case 21: return mA_21;
            case 22: return mA_22;
            case 23: return mA_23;

            case 24: return mA_24;
            case 25: return mA_25;
            case 26: return mA_26;
            case 27: return mA_27;
            case 28: return mA_28;
            case 29: return mA_29;
            case 30: return mA_30;
            case 31: return mA_31;
            default:
              return default(A);
          }
        }
        set {
          switch (i) {
            case 0: mA_0 = value; return;
            case 1: mA_1 = value; return;
            case 2: mA_2 = value; return;
            case 3: mA_3 = value; return;
            case 4: mA_4 = value; return;
            case 5: mA_5 = value; return;
            case 6: mA_6 = value; return;
            case 7: mA_7 = value; return;

            case 8: mA_8 = value; return;
            case 9: mA_9 = value; return;
            case 10: mA_10 = value; return;
            case 11: mA_11 = value; return;
            case 12: mA_12 = value; return;
            case 13: mA_13 = value; return;
            case 14: mA_14 = value; return;
            case 15: mA_15 = value; return;

            case 16: mA_16 = value; return;
            case 17: mA_17 = value; return;
            case 18: mA_18 = value; return;
            case 19: mA_19 = value; return;
            case 20: mA_20 = value; return;
            case 21: mA_21 = value; return;
            case 22: mA_22 = value; return;
            case 23: mA_23 = value; return;

            case 24: mA_24 = value; return;
            case 25: mA_25 = value; return;
            case 26: mA_26 = value; return;
            case 27: mA_27 = value; return;
            case 28: mA_28 = value; return;
            case 29: mA_29 = value; return;
            case 30: mA_30 = value; return;
            case 31: mA_31 = value; return;
            default: return;
          }
        }
      }
    }

    internal struct PairArray<Child> : IStackArray<A> where Child : struct, IStackArray<A> {
      private Child mLeft, mRight;

      public A this[int i] {
        get {
          if (i % 2 == 0) {
            return mLeft[i >> 1];
          }
          else {
            return mRight[i - 1 >> 1];
          }
        }
        set {
          if (i % 2 == 0) {
            mLeft[i >> 1] = value;
          }
          else {
            mRight[i - 1 >> 1] = value;
          }
        }
      }

      public int Length { get { return 2 * mLeft.Length; } }
    }

    // TODO: Kill me
    /*
    private static IStackArray<A> Create<Child>(int size) where Child : struct, IStackArray<A> {
      if (size == 0) {
        return new Child();
      } else {
        return Create<PairArray<Child>>(size >> 1);
      }
    }

    internal static IStackArray<A> Create(int size) {
      if (size == 0) {
        return new EmptyArray();
      } else if (size == 1) {
        return new SingletonArray();
      } else {
        return Create<SingletonArray>(size >> 1);
      }
    }
     */
  }
}
