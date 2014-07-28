'------------------------------------------------------------------------------
' <copyright file="Buufer.vb" company="Microsoft">
'     Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
' <disclaimer>
'     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
'     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
'     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
'     PURPOSE.
' </disclaimer>
'------------------------------------------------------------------------------

Public Class Buffer(Of T)
  Public ReadOnly Put As Asynchronous.Channel(Of T)
  Public ReadOnly Take As Synchronous(Of T).Channel

  Public Function TakeAndPut(ByVal t As T) As T
    Return t
  End Function

  Public Sub New()
    Dim j As Join = Join.Create()
    j.Initialize(Put)
    j.Initialize(Take)
    j.When(Take).And(Put).Do(AddressOf TakeAndPut)
  End Sub

End Class

Public Class OnePlaceBuffer(Of T)
  Private ReadOnly Empty As Asynchronous.Channel
  Private ReadOnly Contains As Asynchronous.Channel(Of T)
  Public ReadOnly Put As Synchronous.Channel(Of T)
  Public ReadOnly Take As Synchronous(Of T).Channel

  Public Sub PutAndEmpty(ByVal t As T)
    Contains(t)
  End Sub

  Public Function TakeAndContains(ByVal t As T) As T
    Empty()
    Return t
  End Function

  Public Sub New()
    Dim j As Join = Join.Create()
    j.Initialize(Empty)
    j.Initialize(Contains)
    j.Initialize(Put)
    j.Initialize(Take)
    j.When(Put).And(Empty).Do(AddressOf PutAndEmpty)
    j.When(Take).And(Contains).Do(AddressOf TakeAndContains)
    Empty()
  End Sub
End Class
