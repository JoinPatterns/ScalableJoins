Imports System
Imports Microsoft.Research.Joins
Public Class Life
    Private pca As LifePCA

    Private UpdateTime As Asynchronous.Channel(Of TimeSpan)

   

    Private Sub SimulateButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SimulateButton.Click
        If pca IsNot Nothing Then
            pca.Halt()
        End If
        pca = New LifePCA(Decimal.ToInt32(pUpDown.Value), Decimal.ToInt32(mUpDown.Value), Decimal.ToInt32(CellSizeUpDown.Value))
        pca.Simulate()
    End Sub



    Private Sub Life_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If pca IsNot Nothing Then
            pca.Halt()
        End If
    End Sub

    Private Sub PauseButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PauseButton.Click
        If pca IsNot Nothing Then
            pca.Pause()
        End If
    End Sub

    Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub
End Class

