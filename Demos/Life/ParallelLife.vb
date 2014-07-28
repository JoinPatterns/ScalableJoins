
Module ParallelLife
    Sub Main()
        Application.Run(New Life())
    End Sub
End Module


Class LifePCA
    Inherits GenericPCA(Of Cell)
    Enum Cell As Byte
        Dead = 0
        Alive = 1
        Random = 2
    End Enum

    Public Overrides Function NextState(ByVal u As Cell()(), ByVal i As Integer, ByVal j As Integer, ByVal r As System.Random) As Cell
       
        Dim Living As Integer = u(i - 1)(j - 1) + u(i - 1)(j) + u(i - 1)(j + 1) + _
                     u(i)(j - 1) + u(i)(j + 1) + _
                     u(i + 1)(j - 1) + u(i + 1)(j) + u(i + 1)(j + 1)


        Select Case u(i)(j)
            Case Cell.Alive
                If Living = 2 Or Living = 3 Then
                    Return Cell.Alive
                Else
                    Return Cell.Dead
                End If
            Case Cell.Dead
                If Living = 3 Then
                    Return Cell.Alive
                Else
                    Return Cell.Dead
                End If
            Case Cell.Random
                If r.Next(0, 10) = 0 Then
                    Return Cell.Alive
                Else
                    Return Cell.Dead
                End If
        End Select
    End Function

    Sub New(ByVal q As Integer, ByVal m As Integer, ByVal CellSize As Integer)
        MyBase.New(q, m, CellSize, Cell.Dead, Cell.Dead, Cell.Dead, Cell.Dead, Cell.Random)
    End Sub

    Public Overrides Function GetColor(ByVal S As Cell) As Color
        Select Case S
            Case Cell.Alive
                Return Color.Red
            Case Cell.Dead
                Return Color.Black
            Case Cell.Random
                Return Color.Blue
        End Select
    End Function
End Class




