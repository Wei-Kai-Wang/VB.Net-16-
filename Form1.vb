Imports System
Imports System.IO.Ports

Public Class Form1
    Private Sub 连接ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 连接ToolStripMenuItem1.Click

    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Form3.Visible = False
        Form2.TopLevel = False
        Form2.FormBorderStyle = FormBorderStyle.None
        Form2.WindowState = FormWindowState.Maximized
        Form2.Parent = Panel1
        Form2.Show()
    End Sub
    Private Sub Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Panel1.Paint

    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        Form2.Visible = False
        Form2.Close()
        Form3.TopLevel = False
        Form3.FormBorderStyle = FormBorderStyle.None
        Form3.WindowState = FormWindowState.Maximized
        Form3.Parent = Panel1
        Form3.Show()
    End Sub
    Private Sub ToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem3.Click

    End Sub
    Public Com2Send As Byte()
    Public Recieve_String() As Integer
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Static count As Integer = 1
        If (Button1.Text = "Start" Or Button1.Text = "未连接") Then
            Try
                Serial_con()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
            If (SerialPort1.IsOpen) Then
                Button1.Text = "Stop"
                Button1.BackColor = Color.Green
                Button1.ForeColor = Color.Red
                'timer设置
                Com2Send = Comforsend(ComSelect())
                Dim i As Integer
                If count = 1 Then
                    For i = 0 To Val（ComboBox7.Text）
                        Form3.Chart1.Series.Add("CH" & i)
                    Next
                End If
                Timer1.Enabled = True
            Else
                Button1.Text = "未连接"
                Button1.BackColor = Color.Red
            End If
        ElseIf (Button1.Text = "Stop") Then
            Timer1.Enabled = False
            Button1.Text = "Start"
            Button1.BackColor = SystemColors.Control
            Button1.ForeColor = Color.Black
            Try
                SerialPort1.Close()
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
            count += 1
        End If

    End Sub
    Public Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '获取端口信息,并添加进下拉菜单
        Dim ports As String() = SerialPort.GetPortNames()
        Dim port As String
        For Each port In ports
            ComboBox1.Items.Add(port)
        Next port
        '初始化控制面板
        ComboBox2.SelectedIndex = 0
        ComboBox3.SelectedIndex = 0
        ComboBox4.SelectedIndex = 0
        ComboBox5.SelectedIndex = 0
        ComboBox6.SelectedIndex = 0
        ComboBox7.SelectedIndex = 0
        ComboBox8.SelectedIndex = 1
        ComboBox1.SelectedIndex = 0
        '加载数值显示画面
        Form2.TopLevel = False
        Form2.FormBorderStyle = FormBorderStyle.None
        Form2.WindowState = FormWindowState.Maximized
        Form2.Parent = Panel1
        Form2.Show()
        '建立一个log文件存储数据
        FileOpen(1, "log.dat", OpenMode.Append, OpenAccess.ReadWrite)
        Print(1, Format(Now(), "yyyy/MM/dd-H:mm:ss,") & vbCrLf)
        FileClose(1)
        Form3.Chart1.Series.RemoveAt(0)
        Form3.Chart1.ChartAreas(0).AxisX.Title = "时间 / min"
        'Form3.Chart1.ChartAreas(0).AxisY.Title = " 数据"
    End Sub
    '串口参数
    Private Sub Serial_con()
        SerialPort1.PortName = ComboBox1.Text '串口名称
        SerialPort1.BaudRate = Val(ComboBox2.Text) '波特率
        SerialPort1.DataBits = Val(ComboBox3.Text) '数据位
        SerialPort1.StopBits = Val(ComboBox5.Text) '停止位
        SerialPort1.Parity = Val(ComboBox4.Text) '校验位
        SerialPort1.Open() '打开串口
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        End
    End Sub
    Public Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Interval = Val(ComboBox8.Text) * 1000
        PortSend(Com2Send)
        Recideal()

    End Sub
    Public Sub Log_file(ByVal LogString As String)
        FileOpen(1, OpenMode.Append, OpenAccess.ReadWrite)
        Print(1, LogString)
        FileClose(1)
    End Sub
    Public Sub Recideal()
        Dim n As Integer
        Dim i As Integer
        Dim rc() As Byte
        Dim RecStr As String = ""
        n = SerialPort1.BytesToRead
        ReDim rc(n)
        ReDim Recieve_String(n)
        Try
            If n > 0 Then
                For i = 0 To (n - 1)
                    rc(i) = SerialPort1.ReadByte()
                    Recieve_String(i) = Val(rc(i))
                    RecStr += rc(i) & ","
                Next
                FileOp(RecStr)
                LCDShow()
                LineDraw()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message & "接收数据错误")
        End Try
    End Sub
    '选择发送命令
    Public Function ComSelect() As String
        Dim ComIndex As Integer
        ComIndex = Val(ComboBox7.Text)
        Select Case ComIndex
            Case 16
                ComSelect = "01 03 00 00 00 10 44 06"
            Case 15
                ComSelect = "01 03 00 00 00 0F 05 CE"
            Case 14
                ComSelect = "01 03 00 00 00 0E C4 0E"
            Case Else
                ComSelect = InputBox("请输入16进制通讯命令：")
        End Select
    End Function
    '将待发送数据转换为16进制
    Public Function Comforsend(ByVal CommandString As String) As Byte()
        Dim TestArray() As String = Split(CommandString) '分割通信数据
        Dim hexBytes() As Byte '定义数组
        ReDim hexBytes(TestArray.Length - 1) '重定义
        Dim i As Integer
        For i = 0 To TestArray.Length - 1
            hexBytes(i) = Val("&h" & TestArray(i))
        Next
        Comforsend = hexBytes
    End Function
    '发送数据
    Public Sub PortSend(ByVal Senddata As Byte())
        Try
            SerialPort1.Write(Senddata, 0, Senddata.Length)
        Catch ex As Exception
            MessageBox.Show(ex.Message & "发送数据错误")
        End Try
    End Sub
    Public Sub FileOp(ByVal fo As String)
        Try
            FileOpen(1, "log.dat", OpenMode.Append, OpenAccess.Write)
            Print(1, fo & vbCrLf)
            FileClose(1)
        Catch ex As Exception
            MessageBox.Show(ex.Message & "读写文件错误")
        End Try
    End Sub
    Public Sub LCDShow()
        Try
            Form2.Label10.Text = Recieve_String(4) + Recieve_String(5) * 0.1 '1
            Form2.Label11.Text = Recieve_String(6) + Recieve_String(7) * 0.1
            Form2.Label12.Text = Recieve_String(8) + Recieve_String(9) * 0.1
            Form2.Label13.Text = Recieve_String(10) + Recieve_String(11) * 0.1
            Form2.Label14.Text = Recieve_String(12) + Recieve_String(13) * 0.1 '5
            Form2.Label15.Text = Recieve_String(14) + Recieve_String(15) * 0.1
            Form2.Label16.Text = Recieve_String(16) + Recieve_String(17) * 0.1
            Form2.Label17.Text = Recieve_String(18) + Recieve_String(19) * 0.1
            Form2.Label18.Text = Recieve_String(20) + Recieve_String(21) * 0.1
            Form2.Label19.Text = Recieve_String(22) + Recieve_String(23) * 0.1 '10
            Form2.Label20.Text = Recieve_String(24) + Recieve_String(25) * 0.1
            Form2.Label21.Text = Recieve_String(26) + Recieve_String(27) * 0.1
            Form2.Label22.Text = Recieve_String(28) + Recieve_String(29) * 0.1
            Form2.Label23.Text = Recieve_String(30) + Recieve_String(31) * 0.1
            Form2.Label24.Text = Recieve_String(32) + Recieve_String(33) * 0.1 '15
            Form2.Label25.Text = Recieve_String(34) + Recieve_String(35) * 0.1 '16
        Catch ex As Exception
            MessageBox.Show(ex.Message & "LCD显示错误")
        End Try
    End Sub
    Public Sub LineDraw()
        Try
            Static Count As Integer = 1
            Dim i As Integer
            Dim yvalue As Double
            For i = 0 To Val(ComboBox7.Text)
                'Form3.Chart1.Series(i).Points.Add()
                yvalue = Recieve_String(i + 3) + Recieve_String(i + 4) * 0.1
                Form3.Chart1.Series(i).Points.AddXY(Format((Count - 1) * Val(ComboBox8.Text) / 60, "0.00"), yvalue)
                Form3.Chart1.Series(i).ChartType = DataVisualization.Charting.SeriesChartType.Line
            Next
            Count += 1
        Catch ex As Exception
            MessageBox.Show(ex.Message & "LineDraw")
        End Try

    End Sub
End Class
