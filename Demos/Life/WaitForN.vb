Imports Microsoft.Research.Joins
Imports System
Class WaitForN
    'Public Synchronous Signal()
    Public ReadOnly Signal As Synchronous.Channel

    'Public Synchronous Wait()
    Public ReadOnly Wait As Synchronous.Channel

    'Private Asynchronous SomePending(ByVal n As Integer)
    Private ReadOnly SomePending As Asynchronous.Channel(Of Integer)

    'Private Asynchronous NonePending()
    Private ReadOnly NonePending As Asynchronous.Channel

    Private Sub CaseSignalAndSomePending(ByVal n As Integer)
        ' When Signal, SomePending
        If n = 1 Then
            NonePending()
        Else
            SomePending(n - 1)
        End If
    End Sub

    Private Sub CaseWaitAndNonePending()
        ' When Wait, NonePending
    End Sub


    Public Sub New(ByVal n As Integer)
        Dim j As Join = Join.Create()
        j.Initialize(Signal)
        j.Initialize(Wait)
        j.Initialize(SomePending)
        j.Initialize(NonePending)
        j.When(Signal).And(SomePending).Do(AddressOf CaseSignalAndSomePending)
        j.When(Wait).And(NonePending).Do(AddressOf CaseWaitAndNonePending)
        SomePending(n)
    End Sub

End Class


Class TimeN
    Private sw As New Stopwatch()

    'Public Synchronous Signal()
    Public ReadOnly Signal As Synchronous.Channel
    Private ReadOnly Done As Asynchronous.Channel(Of System.TimeSpan)

    'Private Asynchronous SomePending(ByVal n As Integer)
    Private ReadOnly SomePending As Asynchronous.Channel(Of Integer)

    Private Sub CaseSignalAndSomePending(ByVal n As Integer)
        ' When Signal, SomePending
        If n = 1 Then
            sw.Stop()
            Dim ts As TimeSpan = sw.Elapsed
            sw.Reset()
            Done(ts)
            ' SomePending(n)
        Else
            SomePending(n - 1)
        End If
    End Sub

    Public Sub New(ByVal n As Integer, ByVal Done As Asynchronous.Channel(Of TimeSpan))
        Dim j As Join = Join.Create()
        Me.Done = Done
        j.Initialize(Signal)
        j.Initialize(SomePending)
        j.When(Signal).And(SomePending).Do(AddressOf CaseSignalAndSomePending)
        sw.Start()
        SomePending(n)
    End Sub

End Class

Class UIAttribute
    Inherits ContinuationAttribute
    Private SC As System.Threading.SynchronizationContext = _
      System.Threading.SynchronizationContext.Current()
    Private Sub Apply(ByVal state As Object)
        DirectCast(state, Continuation)()
    End Sub
    Public Overrides Sub BeginInvoke(ByVal task As Continuation)
        SC.Post(AddressOf Apply, task)
    End Sub
    Public Overrides Sub Invoke(ByVal task As Continuation)
        SC.Send(AddressOf Apply, task)
    End Sub
End Class