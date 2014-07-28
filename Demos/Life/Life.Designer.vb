<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Life
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.pUpDown = New System.Windows.Forms.NumericUpDown
        Me.mUpDown = New System.Windows.Forms.NumericUpDown
        Me.CellSizeUpDown = New System.Windows.Forms.NumericUpDown
        Me.PLabel = New System.Windows.Forms.Label
        Me.mLabel = New System.Windows.Forms.Label
        Me.CellSizeLabel = New System.Windows.Forms.Label
        Me.SimulateButton = New System.Windows.Forms.Button
        Me.PauseButton = New System.Windows.Forms.Button
        CType(Me.pUpDown, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.mUpDown, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.CellSizeUpDown, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pUpDown
        '
        Me.pUpDown.Location = New System.Drawing.Point(55, 22)
        Me.pUpDown.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        Me.pUpDown.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.pUpDown.Name = "pUpDown"
        Me.pUpDown.Size = New System.Drawing.Size(61, 20)
        Me.pUpDown.TabIndex = 0
        Me.pUpDown.Value = New Decimal(New Integer() {4, 0, 0, 0})
        '
        'mUpDown
        '
        Me.mUpDown.Location = New System.Drawing.Point(191, 22)
        Me.mUpDown.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.mUpDown.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.mUpDown.Name = "mUpDown"
        Me.mUpDown.Size = New System.Drawing.Size(61, 20)
        Me.mUpDown.TabIndex = 1
        Me.mUpDown.Value = New Decimal(New Integer() {200, 0, 0, 0})
        '
        'CellSizeUpDown
        '
        Me.CellSizeUpDown.Location = New System.Drawing.Point(332, 22)
        Me.CellSizeUpDown.Maximum = New Decimal(New Integer() {20, 0, 0, 0})
        Me.CellSizeUpDown.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.CellSizeUpDown.Name = "CellSizeUpDown"
        Me.CellSizeUpDown.Size = New System.Drawing.Size(61, 20)
        Me.CellSizeUpDown.TabIndex = 2
        Me.CellSizeUpDown.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'PLabel
        '
        Me.PLabel.AutoSize = True
        Me.PLabel.Location = New System.Drawing.Point(52, 5)
        Me.PLabel.Name = "PLabel"
        Me.PLabel.Size = New System.Drawing.Size(118, 13)
        Me.PLabel.TabIndex = 3
        Me.PLabel.Text = "# nodes per side of grid"
        '
        'mLabel
        '
        Me.mLabel.AutoSize = True
        Me.mLabel.Location = New System.Drawing.Point(188, 5)
        Me.mLabel.Name = "mLabel"
        Me.mLabel.Size = New System.Drawing.Size(105, 13)
        Me.mLabel.TabIndex = 4
        Me.mLabel.Text = "# cells per node side"
        '
        'CellSizeLabel
        '
        Me.CellSizeLabel.AutoSize = True
        Me.CellSizeLabel.Location = New System.Drawing.Point(329, 6)
        Me.CellSizeLabel.Name = "CellSizeLabel"
        Me.CellSizeLabel.Size = New System.Drawing.Size(54, 13)
        Me.CellSizeLabel.TabIndex = 5
        Me.CellSizeLabel.Text = "# cell size"
        '
        'SimulateButton
        '
        Me.SimulateButton.Location = New System.Drawing.Point(462, 19)
        Me.SimulateButton.Name = "SimulateButton"
        Me.SimulateButton.Size = New System.Drawing.Size(75, 23)
        Me.SimulateButton.TabIndex = 6
        Me.SimulateButton.Text = "Simulate"
        Me.SimulateButton.UseVisualStyleBackColor = True
        '
        'PauseButton
        '
        Me.PauseButton.Location = New System.Drawing.Point(560, 19)
        Me.PauseButton.Name = "PauseButton"
        Me.PauseButton.Size = New System.Drawing.Size(75, 23)
        Me.PauseButton.TabIndex = 7
        Me.PauseButton.Text = "Pause"
        Me.PauseButton.UseVisualStyleBackColor = True
        '
        'Life
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(647, 55)
        Me.Controls.Add(Me.PauseButton)
        Me.Controls.Add(Me.SimulateButton)
        Me.Controls.Add(Me.CellSizeLabel)
        Me.Controls.Add(Me.mLabel)
        Me.Controls.Add(Me.PLabel)
        Me.Controls.Add(Me.CellSizeUpDown)
        Me.Controls.Add(Me.mUpDown)
        Me.Controls.Add(Me.pUpDown)
        Me.Name = "Life"
        Me.Text = "Parallel Life"
        Me.TopMost = True
        CType(Me.pUpDown, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.mUpDown, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.CellSizeUpDown, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents pUpDown As System.Windows.Forms.NumericUpDown
    Friend WithEvents mUpDown As System.Windows.Forms.NumericUpDown
    Friend WithEvents CellSizeUpDown As System.Windows.Forms.NumericUpDown
    Friend WithEvents PLabel As System.Windows.Forms.Label
    Friend WithEvents mLabel As System.Windows.Forms.Label
    Friend WithEvents CellSizeLabel As System.Windows.Forms.Label
    Friend WithEvents SimulateButton As System.Windows.Forms.Button
    Friend WithEvents PauseButton As System.Windows.Forms.Button
End Class
