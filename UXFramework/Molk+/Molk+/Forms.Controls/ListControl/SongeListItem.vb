﻿'Public Class SongeListItem : Inherits ListControlItem
'    Friend WithEvents RatingBar2 As ListControlProject_Example.RatingBar
'    Friend WithEvents Label1 As System.Windows.Forms.Label

'    Public Property Rating() As Integer
'        Get
'            Return RatingBar2.Stars
'        End Get
'        Set(ByVal value As Integer)
'            RatingBar2.Stars = value
'        End Set
'    End Property

'    Dim mSong As String = "[Song Name]"
'    Public Property Song() As String
'        Get
'            Return mSong
'        End Get
'        Set(ByVal value As String)
'            mSong = value
'            Refresh()
'        End Set
'    End Property

'    Dim mArtist As String = "[Artist]"
'    Public Property Artist() As String
'        Get
'            Return mArtist
'        End Get
'        Set(ByVal value As String)
'            mArtist = value
'            Refresh()
'        End Set
'    End Property

'    Dim mAlbum As String = "[Album]"
'    Public Property Album() As String
'        Get
'            Return mAlbum
'        End Get
'        Set(ByVal value As String)
'            mAlbum = value
'            Refresh()
'        End Set
'    End Property

'    Dim mDuration As String
'    Public Property Duration() As String
'        Get
'            Return Me.Label1.Text
'        End Get
'        Set(ByVal value As String)
'            Me.Label1.Text = value
'        End Set
'    End Property

'    Private Sub InitializeComponent()
'        Me.Label1 = New System.Windows.Forms.Label()
'        Me.RatingBar2 = New ListControlProject_Example.RatingBar()
'        Me.SuspendLayout()
'        '
'        'Label1
'        '
'        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
'        Me.Label1.AutoSize = True
'        Me.Label1.BackColor = System.Drawing.Color.Transparent
'        Me.Label1.Location = New System.Drawing.Point(433, 34)
'        Me.Label1.Name = "Label1"
'        Me.Label1.Size = New System.Drawing.Size(39, 17)
'        Me.Label1.TabIndex = 5
'        Me.Label1.Text = "00:00"
'        '
'        'RatingBar2
'        '
'        Me.RatingBar2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
'        Me.RatingBar2.BackColor = System.Drawing.Color.Transparent
'        Me.RatingBar2.Location = New System.Drawing.Point(397, 10)
'        Me.RatingBar2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
'        Me.RatingBar2.MaximumSize = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.MinimumSize = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.Name = "RatingBar2"
'        Me.RatingBar2.Size = New System.Drawing.Size(75, 15)
'        Me.RatingBar2.Stars = 3
'        Me.RatingBar2.TabIndex = 4
'        '
'        'SongeListItem
'        '
'        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
'        Me.Controls.Add(Me.Label1)
'        Me.Controls.Add(Me.RatingBar2)
'        Me.Name = "SongeListItem"
'        Me.Controls.SetChildIndex(Me.RatingBar2, 0)
'        Me.Controls.SetChildIndex(Me.Label1, 0)
'        Me.ResumeLayout(False)
'        Me.PerformLayout()

'    End Sub

'    Protected Overrides Sub Paint_DrawButton(gfx As Graphics)

'        Dim fnt As Font = Nothing
'        Dim sz As SizeF = Nothing
'        Dim layoutRect As RectangleF
'        Dim SF As New StringFormat With {.Trimming = StringTrimming.EllipsisCharacter}
'        Dim workingRect As New Rectangle(40, 0, RatingBar2.Left - 40 - 6, Me.Height)

'        ' Draw song name
'        fnt = New Font("Segoe UI Light", 14)
'        sz = gfx.MeasureString(mSong, fnt)
'        layoutRect = New RectangleF(40, 0, workingRect.Width, sz.Height)
'        gfx.DrawString(mSong, fnt, Brushes.Black, layoutRect, SF)

'        ' Draw artist name
'        fnt = New Font("Segoe UI Light", 10)
'        sz = gfx.MeasureString(mArtist, fnt)
'        layoutRect = New RectangleF(42, 30, workingRect.Width, sz.Height)
'        gfx.DrawString(mArtist, fnt, Brushes.Black, layoutRect, SF)

'        ' Draw album name
'        fnt = New Font("Segoe UI Light", 10)
'        sz = gfx.MeasureString(mAlbum, fnt)
'        layoutRect = New RectangleF(42, 49, workingRect.Width, sz.Height)
'        gfx.DrawString(mAlbum, fnt, Brushes.Black, layoutRect, SF)

'        Call MyBase.Paint_DrawButton(gfx)
'    End Sub

'End Class
