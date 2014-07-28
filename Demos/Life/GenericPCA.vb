Imports Microsoft.Research.Joins
Imports System.Diagnostics


MustInherit Class GenericPCA(Of State)
    Private ReadOnly q As Integer  'Number of processors per side of (q x q) processor  grid
    Private ReadOnly m As Integer ' Number of squares per side of each processor's m*m grid, must be even!
    Public ReadOnly n As Integer 'Number of squares on each side of the processor grid
    Public delay As Integer = 0 'For debugging...
    Public ReadOnly CellSize As Integer

    ' u1 - u5 determine the initial values of the top, bottom, right and left borders and interior of the entire grid
    Public u1, u2, u3, u4, u5 As State

    Public Sub New(ByVal q As Integer, ByVal m As Integer, ByVal CellSize As Integer, _
            ByVal u1 As State, ByVal u2 As State, ByVal u3 As State, ByVal u4 As State, ByVal u5 As State)
        Me.q = q
        Me.m = m
        Me.n = m * q
        Me.CellSize = CellSize
        Me.u1 = u1
        Me.u2 = u2
        Me.u3 = u3
        Me.u4 = u4
        Me.u5 = u5
    End Sub

    Public MustOverride Function NextState(ByVal Grid As State()(), ByVal i As Integer, ByVal j As Integer, ByVal r As System.Random) As State

    Public MustOverride Function GetColor(ByVal S As State) As Color

    Function Initial(ByVal i As Integer, ByVal j As Integer) As State
        If i = 0 Then
            Return u1
        ElseIf i = n + 1 Then
            Return u2
        ElseIf j = n + 1 Then
            Return u3
        ElseIf j = 0 Then
            Return u4
        Else
            Return u5
        End If
    End Function

    Class Node
        Inherits Form
        Private PCA As GenericPCA(Of State)
        Dim offscreen As Bitmap
        Private ReadOnly q As Integer  'Number of processors per side of (q x q) processor  grid
        Private ReadOnly m As Integer ' Number of squares per side of each processor's m*m grid, must be even!
        Public ReadOnly n As Integer 'Number of squares on each side of the processor grid
        Public ReadOnly delay As Integer
        Public ReadOnly CellSize As Integer
        Dim qi As Integer
        Dim qj As Integer
        Private r As System.Random

        Dim iterations As Integer = 0
        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            'MyBase.OnPaint(e)
            Using g As Graphics = e.Graphics()
                g.DrawImage(offscreen, Me.ClientRectangle)
            End Using
        End Sub

        ' overridden to reduce flicker
        Protected Overrides Sub OnPaintBackground(ByVal e As System.Windows.Forms.PaintEventArgs)
            'MyBase.OnPaintBackground(e)
        End Sub

        Dim half As Single = 0.5
        Sub ReDraw()
            Dim unow As State()() = u(b)
            For i As Integer = 1 To m
                Dim unowi As State() = unow(i)
                For j As Integer = 1 To m
                    offscreen.SetPixel(j - 1, i - 1, PCA.GetColor(unowi(j)))
                Next
            Next
            Using g As Graphics = Graphics.FromImage(offscreen)
                g.DrawString(iterations.ToString, System.Drawing.SystemFonts.DefaultFont, Brushes.Yellow, m * half, m * half)
                g.DrawRectangle(Pens.White, 0, 0, m, m)
            End Using
        End Sub

        Public Shadows up, down, left, right As Node
       

        'Public Asynchronous TopRow(ByVal Row As State())
        'Public Asynchronous BottomRow(ByVal Row As State())
        'Public Asynchronous LeftColumn(ByVal Column As State())
        'Public Asynchronous RightColumn(ByVal Column As State())

        Public ReadOnly TopRow, BottomRow, LeftColumn, RightColumn As Asynchronous.Channel(Of State())



        Dim u(1)()() As State ' subgrid
        'Asynchronous Start()
        Public ReadOnly Start As Asynchronous.Channel
        Function NewGrid() As State()()
            Dim i0 As Integer = qi * m
            Dim j0 As Integer = qj * m
            Dim u(m + 1)() As State
            For i As Integer = 0 To m + 1
                Dim ui(m + 1) As State
                u(i) = ui
                For j As Integer = 0 To m + 1
                    ui(j) = PCA.Initial(i0 + i, j0 + j)
                Next
            Next
            Return u
        End Function

        Private ReadOnly done As Synchronous.Channel


        Public Sub New(ByVal pca As GenericPCA(Of State), ByVal qi As Integer, ByVal qj As Integer)
            Me.qi = qi
            Me.qj = qj
            Me.PCA = pca
            Me.q = pca.q
            Me.m = pca.m
            Me.n = pca.n

            Me.done = done
            Me.delay = pca.delay
            Me.CellSize = pca.CellSize
            Me.r = New System.Random(qi * m + qj + System.Environment.TickCount)
            MyTopRow = New State(m) {}
            MyRightColumn = New State(m) {}
            MyBottomRow = New State(m) {}
            MyLeftColumn = New State(m) {}
            Dim j As Join = Join.Create(Of Join.LockBased)()
            j.Initialize(Start)
            j.Initialize(StartWorker)
            j.Initialize(Receive)
            j.Initialize(TopRow)
            j.Initialize(RightColumn)
            j.Initialize(BottomRow)
            j.Initialize(LeftColumn)
            j.Initialize(Toggle)
            j.Initialize(Await)
            j.Initialize(Halt)
            j.When(Start).Do(AddressOf CaseStart)
            j.When(StartWorker).Do(AddressOf CaseStartWorker)
            j.When(Receive).And(TopRow).And(RightColumn).And(BottomRow).And(LeftColumn).Do(AddressOf CaseReceiveAndRows)
            j.When(Receive).And(Toggle).Do(AddressOf CaseReceiveAndToggle)
            j.When(Receive).And(Halt).Do(AddressOf CaseReceiveAndHalt)
            j.When(Await).And(Toggle).Do(AddressOf CaseAwaitAndToggle)
            j.When(Await).And(Halt).Do(AddressOf CaseAwaitAndHalt)
        End Sub

        Private Sub CaseStart() 'When Start
            System.Threading.Thread.CurrentThread.IsBackground = True
            Application.Run(Me)
        End Sub

        Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
            MyBase.OnLoad(e)
            Me.Bounds = New Rectangle(0, 0, CellSize * m, CellSize * m)
            Me.SetDesktopLocation(qj * m * CellSize, qi * m * CellSize)
            Me.TopMost = False
            Me.Width = CellSize * m
            Me.Height = CellSize * m
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
            Me.ClientSize = New Size(m * CellSize, m * CellSize)
            offscreen = New Bitmap(m, m)
        End Sub

        Protected Overrides Sub OnCreateControl()
            MyBase.OnCreateControl()
            StartWorker()
        End Sub

        'Asynchronous StartWorker()
        Private ReadOnly StartWorker As Asynchronous.Channel

        Private Halted As Boolean = False
        Private Sub CaseStartWorker() ' When StartWorker
            System.Threading.Thread.CurrentThread.IsBackground = True
            u(0) = NewGrid()
            u(1) = NewGrid()
            While True
                Me.Invoke(New MethodInvoker(AddressOf Animate))
                Exchange()
                If Halted Then Exit While
                If delay > 0 Then System.Threading.Thread.Sleep(delay)
                Relax()
            End While
            Receive() ' Just to respond to Halt and Toggle...
        End Sub
       

        Private Sub Animate()
            ReDraw()
            Refresh()
        End Sub

        Private b As Integer = 0

        Private Sub Relax()
            Dim unow As State()() = u(b)
            Dim unext As State()() = u(1 - b)
            For i As Integer = 1 To m
                Dim unexti As State() = unext(i)
                For j As Integer = 1 To m
                    unexti(j) = PCA.NextState(unow, i, j, r)
                Next
            Next
            ' Swap Grids
            b = 1 - b
            iterations = iterations + 1
        End Sub

        Private Sub Exchange()
            Send()
            Receive()
        End Sub

        Private MyTopRow, MyRightColumn, MyBottomRow, MyLeftColumn As State()

        ' send rows and columns to this node's neighbours, if any
        Private Sub Send()
            Dim ub As State()() = u(b)
            For k As Integer = 1 To m
                MyTopRow(k) = ub(1)(k)
                MyRightColumn(k) = ub(k)(m)
                MyBottomRow(k) = ub(m)(k)
                MyLeftColumn(k) = ub(k)(1)
            Next
            If qi < q - 1 Then down.TopRow(MyBottomRow)
            If qj < q - 1 Then right.LeftColumn(MyRightColumn)
            If qi > 0 Then up.BottomRow(MyTopRow)
            If qj > 0 Then left.RightColumn(MyLeftColumn)
            ' edge nodes send themselves a dummy row or column, to avoid waiting for a non-existing neighbour
            If qi = 0 Then Me.TopRow(MyTopRow)
            If qj = 0 Then Me.LeftColumn(MyLeftColumn)
            If qi = q - 1 Then Me.BottomRow(MyBottomRow)
            If qj = q - 1 Then Me.RightColumn(MyRightColumn)
        End Sub
        ' receive (possibly dummy) rows and columns from this node's neighbours
        'Synchronous Receive()
        Private ReadOnly Receive As Synchronous.Channel

        Private Sub CaseReceiveAndRows(ByVal TopRow As State(), ByVal RightColumn As State(), ByVal BottomRow As State(), ByVal LeftColumn As State())
            ' When Receive, TopRow, RightColumn, BottomRow, LeftColumn
            MyTopRow = TopRow
            MyRightColumn = RightColumn
            MyBottomRow = BottomRow
            MyLeftColumn = LeftColumn
            Dim ub As State()() = u(b)
            For k As Integer = 1 To m
                If qi > 0 Then ub(0)(k) = TopRow(k)
                If qj > 0 Then ub(k)(0) = LeftColumn(k)
                If qi < q - 1 Then ub(m + 1)(k) = BottomRow(k)
                If qj < q - 1 Then ub(k)(m + 1) = RightColumn(k)
            Next
        End Sub

        'Public Asynchronous Halt(ByVal WaitForN As WaitForN)
        Public ReadOnly Halt As Asynchronous.Channel(Of WaitForN)
        Private Sub CaseReceiveAndHalt(ByVal WaitForN As WaitForN)
            'When Receive, Halt
            Me.BeginInvoke(New MethodInvoker(AddressOf Me.Close))
            WaitForN.Signal()
            Halted = True
        End Sub
        'Public Asynchronous Toggle()
        Public ReadOnly Toggle As Asynchronous.Channel
        Private Sub CaseReceiveAndToggle()
            'When Receive, Toggle
            Await()
        End Sub
        Sub ClickHandler(ByVal s As Object, ByVal o As System.EventArgs) Handles Me.Click
            Toggle()
        End Sub
        'Private Synchronous Await()
        Private ReadOnly Await As Synchronous.Channel
        Private Sub CaseAwaitAndToggle()
            'When Await, Toggle
            Receive()
        End Sub

        Private Sub CaseAwaitAndHalt(ByVal WaitForN As WaitForN)
            'When Await, Halt
            Halt(WaitForN)
            Receive()
        End Sub

    End Class

    Private Nodes()() As Node

    Private sw As Stopwatch = New Stopwatch()

    ' Starts a simulation running
    Sub Simulate()
        Dim Nodes(q - 1)() As Node
        For i As Integer = 0 To q - 1
            Nodes(i) = New Node(q - 1) {}
            For j As Integer = 0 To q - 1
                Nodes(i)(j) = New Node(Me, i, j)
            Next
        Next
        For i As Integer = 0 To q - 1
            For j As Integer = 0 To q - 1
                If i > 0 Then Nodes(i)(j).up = Nodes(i - 1)(j)
                If j < q - 1 Then Nodes(i)(j).right = Nodes(i)(j + 1)
                If i < q - 1 Then Nodes(i)(j).down = Nodes(i + 1)(j)
                If j > 0 Then Nodes(i)(j).left = Nodes(i)(j - 1)
            Next
        Next
        For i As Integer = 0 To q - 1
            For j As Integer = 0 To q - 1
                Nodes(i)(j).Start()
            Next
        Next
        Me.Nodes = Nodes

    End Sub

    ' sends an asynchronous halt command to each node and waits for a reply from each of them.
    Public Sub Halt()
        Dim WaitForN As WaitForN = New WaitForN(q * q)
        For i As Integer = 0 To q - 1
            For j As Integer = 0 To q - 1
                Nodes(i)(j).Halt(WaitForN)
            Next
        Next
        WaitForN.Wait()
    End Sub
    ' sends an asynchronous command to each node to toggle execution (pause and unpause)
    Public Sub Pause()
        For i As Integer = 0 To q - 1
            For j As Integer = 0 To q - 1
                Nodes(i)(j).Toggle()
            Next
        Next
    End Sub

    Sub New()
    End Sub
End Class

