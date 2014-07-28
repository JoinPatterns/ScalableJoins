//------------------------------------------------------------------------------
// <copyright file="BitMasks.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <disclaimer>
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//     PURPOSE.
// </disclaimer>
//------------------------------------------------------------------------------


namespace Microsoft.Research.Joins.BitMasks {

  public interface IIntSet<S> {
    int Capacity();
    S Empty();
    void Add(int b);
    void AddAll(S s);
    void Remove(int b);
    void RemoveAll(S s);
    bool Contains(int b);
    bool ContainsAll(S s);
    bool IsEmpty();
    S Difference(S s);

  }

  internal struct IntSet32 : IIntSet<IntSet32> {
    private uint v;
    int IIntSet<IntSet32>.Capacity() { return 32; }
    IntSet32 IIntSet<IntSet32>.Empty() { return new IntSet32(); }
    void IIntSet<IntSet32>.Add(int bit) { v |= ((uint)1 << bit); }
    void IIntSet<IntSet32>.Remove(int m) { v &= ~((uint)1 << m); }
    void IIntSet<IntSet32>.RemoveAll(IntSet32 m) { v &= ~m.v; }
    void IIntSet<IntSet32>.AddAll(IntSet32 s) { v |= s.v; }
    bool IIntSet<IntSet32>.Contains(int bit) { return (~v & ((uint)1 << bit)) == 0; }
    bool IIntSet<IntSet32>.ContainsAll(IntSet32 m) { return (~v & m.v) == 0; }
    bool IIntSet<IntSet32>.IsEmpty() { return v == 0; }
    IntSet32 IIntSet<IntSet32>.Difference(IntSet32 s) {
      IntSet32 b;
      b.v = v & (~s.v);
      return b;
    }

  }

  internal struct IntSet64 : IIntSet<IntSet64> {
    private ulong v;
    int IIntSet<IntSet64>.Capacity() { return 64; }
    IntSet64 IIntSet<IntSet64>.Empty() { return new IntSet64(); }
    void IIntSet<IntSet64>.Add(int bit) { v |= ((ulong)1 << bit); }
    void IIntSet<IntSet64>.Remove(int m) { v &= ~((ulong)1 << m); }
    void IIntSet<IntSet64>.RemoveAll(IntSet64 m) { v &= ~m.v; }
    void IIntSet<IntSet64>.AddAll(IntSet64 s) { v |= s.v; }
    bool IIntSet<IntSet64>.Contains(int bit) { return (~v & ((ulong)1 << bit)) == 0; }
    bool IIntSet<IntSet64>.ContainsAll(IntSet64 m) { return (~v & m.v) == 0; }
    bool IIntSet<IntSet64>.IsEmpty() { return v == 0; }
    IntSet64 IIntSet<IntSet64>.Difference(IntSet64 s) {
      IntSet64 b;
      b.v = v & (~s.v);
      return b;
    }

  }


  internal struct PairSet<S> : IIntSet<PairSet<S>>
    where S : struct, IIntSet<S> {
    private S v0, v1;

    int IIntSet<PairSet<S>>.Capacity() {
      return v0.Capacity() * 2; 
    }
    
    PairSet<S> IIntSet<PairSet<S>>.Empty() {
      PairSet<S> p;
      p.v0 = v0.Empty();
      p.v1 = v1.Empty();
      return p;
    }

    void IIntSet<PairSet<S>>.Add(int bit) {
      if ((bit & 1) == 1) v1.Add(bit >> 1);
      else v0.Add(bit >> 1);
    }
    void IIntSet<PairSet<S>>.Remove(int bit) {
      if ((bit & 1) == 1) v1.Remove(bit >> 1);
      else v0.Remove(bit >> 1);
    }
    void IIntSet<PairSet<S>>.RemoveAll(PairSet<S> m) {
      v0.RemoveAll(m.v0);
      v1.RemoveAll(m.v1);
    }
    void IIntSet<PairSet<S>>.AddAll(PairSet<S> m) {
      v0.AddAll(m.v0);
      v1.AddAll(m.v1);
    }
    bool IIntSet<PairSet<S>>.Contains(int bit) {
      if ((bit & 1) == 1)
        return v1.Contains(bit >> 1);
      else return v0.Contains(bit >> 1);
    }

    bool IIntSet<PairSet<S>>.ContainsAll(PairSet<S> m) {
      return v0.ContainsAll(m.v0) && v1.ContainsAll(m.v1);
    }
   
    bool IIntSet<PairSet<S>>.IsEmpty() { return v0.IsEmpty() && v1.IsEmpty(); }
    PairSet<S> IIntSet<PairSet<S>>.Difference(PairSet<S> s) {
      PairSet<S> p;
      p.v0 = v0.Difference(s.v0);
      p.v1 = v1.Difference(s.v1);
      return p;
    }


  }



}