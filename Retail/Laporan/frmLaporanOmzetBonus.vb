﻿Imports System.Data.SqlClient
Imports System.Data
Imports DevExpress.XtraEditors
Imports VPoint.mdlCetakCR
Imports VPoint.Ini
Imports DevExpress.Utils
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.XtraEditors.Repository

Public Class frmLaporanOmzetBonus
    Dim SQL As String

    Dim repckedit As New RepositoryItemCheckEdit
    Dim reppicedit As New RepositoryItemPictureEdit
    Dim IsPeriodeLock As Boolean = False

    Private Sub SimpleButton6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdTutup.Click
        Me.Close()
    End Sub

    Private Sub frmLaporanOmzetBonus_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            GV1.SaveLayoutToXml(FolderLayouts & Me.Name & GV1.Name & ".xml")
        Catch ex As Exception

        End Try
    End Sub

    Private Sub frmLaporanOmzetBonus_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            txtPeriode.DateTime = TanggalSystem
            cmdRefresh.PerformClick()
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Sub SimpleButton4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPObatPKP.Click
        Dim NamaFile As String = Application.StartupPath & "\Report\PromosiBulanan.rpt"
        Try
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=2&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=True")
            Else
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=2&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=True")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Sub SimpleButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOmzetBulanan.Click
        'XtraMessageBox.Show("Maaf, laporan sedang proses pembuatan.", NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Dim NamaFile As String = Application.StartupPath & "\Report\OmzetHarian.rpt"
        Dim dlg As WaitDialogForm = Nothing
        Try
            dlg = New WaitDialogForm("Proses menghitung ...", NamaAplikasi)
            dlg.Show()
            dlg.TopMost = True
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "")
            Else
                EksekusiSQL("UPDATE MLapOmzet SET Bank=IsNull(Bank,0) WHERE MONTH(Periode)=" & Month(txtPeriode.DateTime) & " AND YEAR(Periode)=" & Year(txtPeriode.DateTime))
                EksekusiSQL("UPDATE MLapOmzet SET Setoran=IsNull(Setoran,0) WHERE MONTH(Periode)=" & Month(txtPeriode.DateTime) & " AND YEAR(Periode)=" & Year(txtPeriode.DateTime))
                'HitungOmzetHarian(False)
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "IDUser=" & IDUserAktif & "&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If Not dlg Is Nothing Then
                dlg.Close()
            End If
            dlg.Dispose()
        End Try
    End Sub

    Private Function ViewReportLocal(ByVal frmParent As XtraForm, ByVal Action As action_, ByVal sReportName As String, ByVal Judul As String, Optional ByVal sSelectionFormula As String = "", Optional ByVal param As String = "", Optional ByVal Formula As String = "", Optional ByVal SortOrder As String = "") As Boolean
        Dim intCounter As Integer
        Dim intCounter1 As Integer
        Dim objReport As New CrystalDecisions.CrystalReports.Engine.ReportDocument
        Dim ConInfo As New CrystalDecisions.Shared.TableLogOnInfo

        Dim paraValue As New CrystalDecisions.Shared.ParameterDiscreteValue
        Dim currValue As CrystalDecisions.Shared.ParameterValues
        Dim mySubReportObject As CrystalDecisions.CrystalReports.Engine.SubreportObject
        Dim mySubRepDoc As New CrystalDecisions.CrystalReports.Engine.ReportDocument
        Dim FieldDef As CrystalDecisions.CrystalReports.Engine.FieldDefinition

        Dim strParamenters As String = param
        Dim strParValPair() As String
        Dim strVal() As String
        Dim sFileName As String = ""

        Dim sFormulaName() As String
        Dim sFormulaValues() As String
        'Dim index As Integer=
        Dim dlg As WaitDialogForm
        'Dim frmctk As frmCetakMDI = Nothing
        dlg = New WaitDialogForm("Sedang diproses...", "Mohon Tunggu Sebentar.")
        dlg.Show()
        Try

            sFileName = sReportName 'DownloadReport(sReportName, m_strReportDir)
            If Action = action_.Edit Then
                dlg.Close()
                dlg.Dispose()
                BukaFile(sFileName)
                Exit Try
            End If
            objReport.Load(sFileName)

            intCounter = objReport.DataDefinition.ParameterFields.Count
            If intCounter = 1 Then
                If InStr(objReport.DataDefinition.ParameterFields(0).ParameterFieldName, ".", CompareMethod.Text) > 0 Then
                    intCounter = 0
                End If
            End If

            If intCounter > 0 And Trim(param) <> "" Then
                strParValPair = strParamenters.Split("&")
                For index = 0 To UBound(strParValPair)
                    If InStr(strParValPair(index), "=") > 0 Then
                        strVal = strParValPair(index).Split("=")
                        paraValue.Value = strVal(1)
                        currValue = objReport.DataDefinition.ParameterFields(strVal(0)).CurrentValues
                        currValue.Add(paraValue)
                        objReport.DataDefinition.ParameterFields(strVal(0)).ApplyCurrentValues(currValue)
                    End If
                Next
            End If

            ConInfo.ConnectionInfo.UserID = BacaIni("odbcconfig", "Username", "sa")
            ConInfo.ConnectionInfo.Password = BacaIni("odbcconfig", "Password", "sahaysstem")
            ConInfo.ConnectionInfo.ServerName = BacaIni("odbcconfig", "Server", "CityToys")
            ConInfo.ConnectionInfo.DatabaseName = BacaIni("odbcconfig", "Database", "DBCityToys")
            ConInfo.ConnectionInfo.AllowCustomConnection = True

            For intCounter = 0 To objReport.Database.Tables.Count - 1
                objReport.Database.Tables(intCounter).ApplyLogOnInfo(ConInfo)
            Next

            For index As Integer = 0 To objReport.ReportDefinition.Sections.Count - 1
                For intCounter = 0 To objReport.ReportDefinition.Sections(index).ReportObjects.Count - 1
                    With objReport.ReportDefinition.Sections(index)
                        If .ReportObjects(intCounter).Kind = CrystalDecisions.Shared.ReportObjectKind.SubreportObject Then
                            mySubReportObject = CType(.ReportObjects(intCounter), CrystalDecisions.CrystalReports.Engine.SubreportObject)
                            mySubRepDoc = mySubReportObject.OpenSubreport(mySubReportObject.SubreportName)
                            For intCounter1 = 0 To mySubRepDoc.Database.Tables.Count - 1
                                mySubRepDoc.Database.Tables(intCounter1).ApplyLogOnInfo(ConInfo)
                            Next
                        End If
                    End With
                Next
            Next
            If sSelectionFormula.Length > 0 Then
                objReport.RecordSelectionFormula = sSelectionFormula
            End If

            For xx As Integer = 0 To objReport.DataDefinition.FormulaFields.Count - 1
                If objReport.DataDefinition.FormulaFields(xx).Name = "NamaPerusahaan" Then
                    objReport.DataDefinition.FormulaFields("NamaPerusahaan").Text = "'" & NamaPerusahaan.ToUpper.Replace(Chr(13), "'+ CHR(13) +'").Replace(Chr(10), "'+ CHR(10) +'") & "'"
                ElseIf objReport.DataDefinition.FormulaFields(xx).Name = "AlamatPerusahaan" Then
                    objReport.DataDefinition.FormulaFields("AlamatPerusahaan").Text = "'" & AlamatPerusahaan.ToUpper.Replace(Chr(13), "'+ CHR(13) +'").Replace(Chr(10), "'+ CHR(10) +'") & "'"
                ElseIf objReport.DataDefinition.FormulaFields(xx).Name = "KotaPerusahaan" Then
                    objReport.DataDefinition.FormulaFields("KotaPerusahaan").Text = "'" & "Surabaya".ToUpper.Replace(Chr(13), "'+ CHR(13) +'").Replace(Chr(10), "'+ CHR(10) +'") & "'"
                End If
            Next

            If Formula.ToString.Length >= 1 Then
                sFormulaName = Formula.ToString.Split("&")
                For i As Integer = 0 To sFormulaName.Length - 1
                    sFormulaValues = sFormulaName(i).ToString.Split("=")
                    objReport.DataDefinition.FormulaFields(sFormulaValues(0).ToString).Text = sFormulaValues(1).ToString
                Next
            End If
            Dim strDB As String()
            If SortOrder.ToString.Length >= 1 Then
                strParValPair = SortOrder.Split("&")
                For index As Integer = 0 To UBound(strParValPair)
                    If InStr(strParValPair(index), "=") > 0 Then
                        strVal = strParValPair(index).Split("=")
                        strDB = strVal(0).Split(".")
                        FieldDef = objReport.Database.Tables(strDB(0)).Fields(strDB(1))
                        objReport.DataDefinition.SortFields.Item(index).Field = FieldDef
                        If strVal(1).ToString.ToUpper = "Descending".ToUpper Then
                            objReport.DataDefinition.SortFields(index).SortDirection = CrystalDecisions.Shared.SortDirection.DescendingOrder
                        Else
                            objReport.DataDefinition.SortFields(index).SortDirection = CrystalDecisions.Shared.SortDirection.AscendingOrder
                        End If
                    End If
                Next
            End If
            'If sSelectionFormula.Length > 0 Then
            '    o = sSelectionFormula
            'End If
            Application.DoEvents()
            CrViewer.ReportSource = Nothing
            CrViewer.ReportSource = objReport
            CrViewer.RefreshReport()

            If Action = action_.Preview Then
                CrViewer.Show()
            Else
                CrViewer.PrintReport()
            End If
            Return True
        Catch ex As System.Exception
            XtraMessageBox.Show("Kesalahan : " & ex.Message & " file " & sFileName.ToString)
        Finally
            dlg.Close()
            dlg.Dispose()
        End Try
    End Function

    Private Sub SimpleButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPSewaPKP.Click
        Dim NamaFile As String = Application.StartupPath & "\Report\PromosiBulanan.rpt"
        Try
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=1&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=True")
            Else
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=1&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=True")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Sub SimpleButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPPNBulanan.Click
        'XtraMessageBox.Show("Maaf, laporan sedang proses pembuatan.", NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Dim NamaFile As String = Application.StartupPath & "\Report\PPNBulanan.rpt"
        Dim dlg As WaitDialogForm = Nothing
        Try
            dlg = New WaitDialogForm("Proses menghitung ...", NamaAplikasi)
            dlg.Show()
            dlg.TopMost = True
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "")
            Else
                EksekusiSQL("UPDATE MLapOmzet SET Bank=IsNull(Bank,0) WHERE MONTH(Periode)=" & Month(txtPeriode.DateTime) & " AND YEAR(Periode)=" & Year(txtPeriode.DateTime))
                EksekusiSQL("UPDATE MLapOmzet SET Setoran=IsNull(Setoran,0) WHERE MONTH(Periode)=" & Month(txtPeriode.DateTime) & " AND YEAR(Periode)=" & Year(txtPeriode.DateTime))
                'HitungOmzetHarian(False)
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "IDUser=" & IDUserAktif & "&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If Not dlg Is Nothing Then
                dlg.Close()
            End If
            dlg.Dispose()
        End Try
    End Sub

    Private Sub SimpleButton5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPObatBulanan.Click
        'XtraMessageBox.Show("Maaf, laporan sedang proses pembuatan.", NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Dim NamaFile As String = Application.StartupPath & "\Report\ObatHarian2.rpt"
        Dim dlg As WaitDialogForm = Nothing
        Try
            dlg = New WaitDialogForm("Proses menghitung ...", NamaAplikasi)
            dlg.Show()
            dlg.TopMost = True
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "")
            Else
                HitungOmzetHarian(False)
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "IDUser=" & IDUserAktif & "&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If Not dlg Is Nothing Then
                dlg.Close()
            End If
            dlg.Dispose()
        End Try
    End Sub

    Private Sub SimpleButton7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPObatHarian.Click
        'XtraMessageBox.Show("Maaf, laporan sedang proses pembuatan.", NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Dim NamaFile As String = Application.StartupPath & "\Report\ObatHarian.rpt"
        Dim dlg As WaitDialogForm = Nothing
        Try
            dlg = New WaitDialogForm("Proses menghitung ...", NamaAplikasi)
            dlg.Show()
            dlg.TopMost = True
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "")
            Else
                HitungOmzetHarian(False)
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "IDUser=" & IDUserAktif & "&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If Not dlg Is Nothing Then
                dlg.Close()
            End If
            dlg.Dispose()
        End Try
    End Sub

    'Private Sub HitungObatHarian(Optional ByVal IsHapus As Boolean = True)
    '    Dim SQL As String = ""
    '    Dim cn As New SqlConnection
    '    Dim com As New SqlCommand
    '    Dim oDA As New SqlDataAdapter
    '    Dim ds As New DataSet
    '    Dim TglSampai As Date
    '    Dim where As String = ""
    '    TglSampai = DateAdd(DateInterval.Day, (1 - txtPeriode.DateTime.Day), txtPeriode.DateTime)
    '    TglSampai = DateAdd(DateInterval.Month, 1, TglSampai)
    '    TglSampai = DateAdd(DateInterval.Day, -1, TglSampai)
    '    Dim Hasil As Double = 0.0, TglRun As Date = TanggalSystem
    '    Try
    '        cn.ConnectionString = StrKonSql
    '        cn.Open()
    '        com.Connection = cn
    '        oDA.SelectCommand = com
    '        If IsHapus Then
    '            SQL = "DELETE FROM MLapOmzet WHERE IDUser=" & IDUserAktif
    '            com.CommandText = SQL
    '            com.ExecuteNonQuery()
    '        End If
    '        For i = 1 To TglSampai.Day
    '            TglRun = CDate(txtPeriode.DateTime.ToString("yyyy,MM,") & i.ToString("00"))
    '            If IsHapus Then
    '                SQL = "INSERT INTO MLapOmzet (IDUser,Periode,Tanggal) VALUES (" & IDUserAktif & ",'" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "','" & TglRun.ToString("yyyy-MM-dd") & "')"
    '                com.CommandText = SQL
    '                com.ExecuteNonQuery()
    '            End If
    '            where = " MLapOmzet.IDUser=" & IDUserAktif & " AND MLapOmzet.Periode='" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "' AND MLapOmzet.Tanggal='" & TglRun.ToString("yyyy-MM-dd") & "'"

    '            'OBat Harian PKP
    '            SQL = "SELECT SUM(MJualD.Jumlah) AS Total " & vbCrLf & _
    '                  " FROM MJual " & vbCrLf & _
    '                  " INNER JOIN MJualD ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
    '                  " INNER JOIN MAlamat ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
    '                  " INNER JOIN MBarang ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
    '                  " WHERE MBarang.IDKategori=34 AND IsNull(MAlamat.IsPKP,0)=1 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MJual.Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "'"
    '            com.CommandText = SQL
    '            Hasil = NullToDbl(com.ExecuteScalar)

    '            SQL = "UPDATE MLapOmzet SET ObatPKP=" & FixKoma(Hasil) & " WHERE " & where
    '            com.CommandText = SQL
    '            com.ExecuteNonQuery()

    '            'OBat Harian Non PKP
    '            SQL = "SELECT SUM(MJualD.Jumlah) AS Total " & vbCrLf & _
    '                  " FROM MJual " & vbCrLf & _
    '                  " INNER JOIN MJualD ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
    '                  " INNER JOIN MAlamat ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
    '                  " INNER JOIN MBarang ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
    '                  " WHERE MBarang.IDKategori=34 AND IsNull(MAlamat.IsPKP,0)=0 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MJual.Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "'"
    '            com.CommandText = SQL
    '            Hasil = NullToDbl(com.ExecuteScalar)

    '            SQL = "UPDATE MLapOmzet SET ObatNPKP=" & FixKoma(Hasil) & " WHERE " & where
    '            com.CommandText = SQL
    '            com.ExecuteNonQuery()
    '        Next
    '    Catch ex As Exception
    '        XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
    '    Finally
    '        If cn.State = ConnectionState.Open Then
    '            cn.Close()
    '        End If
    '        cn.Dispose()
    '        com.Dispose()
    '        oDA.Dispose()
    '        ds.Dispose()
    '    End Try
    'End Sub

    Private Sub HitungOmzetHarian(Optional ByVal IsHapus As Boolean = True, Optional ByVal HitungUlangZReport As Boolean = False)
        Dim SQL As String = ""
        Dim cn As New SqlConnection
        Dim com As New SqlCommand
        Dim oDA As New SqlDataAdapter
        Dim ds As New DataSet
        Dim TglSampai As Date
        Dim where As String = ""
        TglSampai = DateAdd(DateInterval.Day, (1 - txtPeriode.DateTime.Day), txtPeriode.DateTime)
        TglSampai = DateAdd(DateInterval.Month, 1, TglSampai)
        TglSampai = DateAdd(DateInterval.Day, -1, TglSampai)
        Dim Hasil As Double = 0.0, TglRun As Date = TanggalSystem, NonBKP As Double = 0.0, BKP As Double = 0.0
        Dim dlg As WaitDialogForm = Nothing
        Try
            dlg = New WaitDialogForm("Proses menghitung ...", NamaAplikasi)
            dlg.Show()
            dlg.TopMost = True
            cn.ConnectionString = StrKonSql
            cn.Open()
            com.Connection = cn
            com.CommandTimeout = cn.ConnectionTimeout
            oDA.SelectCommand = com
            ProgressBarControl1.Visible = True
            ProgressBarControl1.Position = 0
            ProgressBarControl1.Refresh()
            If IsHapus Then
                SQL = "DELETE FROM MLapOmzet WHERE Periode='" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "'"
                com.CommandText = SQL
                com.ExecuteNonQuery()
                ProgressBarControl1.Position = (1 / TglSampai.Day) * 100
                ProgressBarControl1.Refresh()
            End If
            For i = 1 To TglSampai.Day
                TglRun = CDate(txtPeriode.DateTime.ToString("yyyy,MM,") & i.ToString("00"))
                where = " MLapOmzet.Periode='" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "' AND MLapOmzet.Tanggal='" & TglRun.ToString("yyyy-MM-dd") & "'"
                If HitungUlangZReport AndAlso IsHapus Then
                    SQL = "INSERT INTO MLapOmzet (IDUser,Periode,Tanggal) VALUES (" & IDUserAktif & ",'" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "','" & TglRun.ToString("yyyy-MM-dd") & "')"
                    com.CommandText = SQL
                    com.ExecuteNonQuery()
                Else
                    com.CommandText = "SELECT * FROM MLapOmzet WHERE " & where
                    If NullToLong(com.ExecuteScalar()) = 0 Then 'Tidak ada
                        SQL = "INSERT INTO MLapOmzet (IDUser,Periode,Tanggal,Tunai,Bank,Setoran) VALUES (" & IDUserAktif & ",'" & txtPeriode.DateTime.ToString("yyyy-MM-01") & "','" & TglRun.ToString("yyyy-MM-dd") & "',0,0,0)"
                        com.CommandText = SQL
                        com.ExecuteNonQuery()
                    Else
                        com.CommandText = "UPDATE MLapOmzet SET IDUser=" & IDUserAktif & " WHERE " & where
                        com.ExecuteNonQuery()
                    End If
                End If

                If HitungUlangZReport Then
                    'Omzet Harian
                    SQL = "SELECT SUM(vRekapPenjualanPerDepartemenBersih.Jumlah) AS Total" & vbCrLf & _
                          " FROM vRekapPenjualanPerDepartemenBersih " & vbCrLf & _
                          " WHERE Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' " & vbCrLf & _
                          " AND vRekapPenjualanPerDepartemenBersih.NoID<>28"
                    com.CommandText = SQL
                    Hasil = NullToDbl(com.ExecuteScalar)
                    Application.DoEvents()

                    'Omzet BPK
                    SQL = "SELECT SUM(vRekapPenjualanPerDepartemenBersih.Jumlah) AS Total" & vbCrLf & _
                          " FROM vRekapPenjualanPerDepartemenBersih " & vbCrLf & _
                          " WHERE Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' " & vbCrLf & _
                          " AND vRekapPenjualanPerDepartemenBersih.NoID NOT IN (28, 25, 27, 31, 32)"
                    com.CommandText = SQL
                    BKP = NullToDbl(com.ExecuteScalar)
                    Application.DoEvents()

                    'Omzet Non BPK
                    SQL = "SELECT SUM(vRekapPenjualanPerDepartemenBersih.Jumlah) AS Total" & vbCrLf & _
                          " FROM vRekapPenjualanPerDepartemenBersih " & vbCrLf & _
                          " WHERE Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' " & vbCrLf & _
                          " AND vRekapPenjualanPerDepartemenBersih.NoID IN (25, 27, 31, 32)"
                    com.CommandText = SQL
                    NonBKP = NullToDbl(com.ExecuteScalar)

                    SQL = "UPDATE MLapOmzet SET Tunai=" & FixKoma(Hasil) & ", BKP=" & FixKoma(BKP) & ", NBKP=" & FixKoma(NonBKP) & " WHERE " & where
                    com.CommandText = SQL
                    com.ExecuteNonQuery()
                End If
                If i = TglSampai.Day Then 'Obat PKP dihitung di tgl Terakhir Satu Bulan
                    'Obat Harian PKP
                    SQL = "SELECT SUM(MJualD.Jumlah) AS Total, SUM(CASE WHEN IsNull(MJual.IDTypePajak,0)=2 THEN MJualD.Jumlah ELSE ROUND(MJualD.Jumlah/1.1,0) END) AS DPP, SUM(CASE WHEN IsNull(MJual.IDTypePajak,0)=2 THEN ROUND(MJualD.Jumlah*10/100,0) WHEN IsNull(MJual.IDTypePajak,0)=0 THEN 0 ELSE MJualD.Jumlah-ROUND(MJualD.Jumlah/1.1,0) END) AS PPN " & vbCrLf & _
                          " FROM MJual (NOLOCK) " & vbCrLf & _
                          " INNER JOIN MJualD (NOLOCK) ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
                          " INNER JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
                          " INNER JOIN MBarang (NOLOCK) ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
                          " WHERE MBarang.IDKategori=34 AND IsNull(MAlamat.IsPKP,0)=1 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-01") & "' AND MJual.Tanggal<'" & TglSampai.AddDays(1).ToString("yyyy-MM-dd") & "'"
                    com.CommandText = SQL
                    oDA.SelectCommand = com
                    If Not ds.Tables("MObat") Is Nothing Then
                        ds.Tables("MObat").Clear()
                    End If
                    oDA.Fill(ds, "MObat")
                    Application.DoEvents()
                    If ds.Tables("MObat").Rows.Count >= 1 Then
                        SQL = "UPDATE MLapOmzet SET " & vbCrLf & _
                              " ObatPKP=" & FixKoma(NullToDbl(ds.Tables("MObat").Rows(0).Item("Total"))) & ", " & vbCrLf & _
                              " PPNOBat=" & FixKoma(NullToDbl(ds.Tables("MObat").Rows(0).Item("PPN"))) & ", " & vbCrLf & _
                              " DPPOBat=" & FixKoma(NullToDbl(ds.Tables("MObat").Rows(0).Item("DPP"))) & " " & vbCrLf & _
                              " WHERE " & where
                    Else
                        SQL = "UPDATE MLapOmzet SET ObatPKP=0, DPPObat=0, PPNObat=0 WHERE " & where
                    End If
                    com.CommandText = SQL
                    com.ExecuteNonQuery()
                Else
                    SQL = "UPDATE MLapOmzet SET ObatPKP=0, DPPObat=0, PPNObat=0 WHERE " & where
                    com.CommandText = SQL
                    com.ExecuteNonQuery()
                End If
                'Obat Harian Non PKP
                SQL = "SELECT SUM(MJualD.Jumlah) AS Total " & vbCrLf & _
                      " FROM MJual (NOLOCK) " & vbCrLf & _
                      " INNER JOIN MJualD (NOLOCK) ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
                      " INNER JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
                      " INNER JOIN MBarang (NOLOCK) ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
                      " WHERE MBarang.IDKategori=34 AND IsNull(MAlamat.IsPKP,0)=0 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MJual.Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "'"
                com.CommandText = SQL
                Hasil = NullToDbl(com.ExecuteScalar)
                Application.DoEvents()

                SQL = "UPDATE MLapOmzet SET ObatNPKP=" & FixKoma(Hasil) & " WHERE " & where
                com.CommandText = SQL
                com.ExecuteNonQuery()

                'Promosi Harian PKP
                SQL = "SELECT SUM(MJualD.Jumlah) AS Total, SUM(CASE WHEN IsNull(MJual.IDTypePajak,0)=2 THEN MJualD.Jumlah ELSE ROUND(MJualD.Jumlah/1.1,0) END) AS DPP, SUM(CASE WHEN IsNull(MJual.IDTypePajak,0)=2 THEN ROUND(MJualD.Jumlah*10/100,0) WHEN IsNull(MJual.IDTypePajak,0)=0 THEN 0 ELSE MJualD.Jumlah-ROUND(MJualD.Jumlah/1.1,0) END) AS PPN " & vbCrLf & _
                      " FROM MJual (NOLOCK) " & vbCrLf & _
                      " INNER JOIN MJualD (NOLOCK) ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
                      " INNER JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
                      " INNER JOIN MBarang (NOLOCK) ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
                      " WHERE MBarang.IDKategori=28 AND IsNull(MAlamat.IsPKP,0)=1 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MJual.Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "'"
                com.CommandText = SQL
                oDA.SelectCommand = com
                If Not ds.Tables("MPromosi") Is Nothing Then
                    ds.Tables("MPromosi").Clear()
                End If
                oDA.Fill(ds, "MPromosi")
                Application.DoEvents()
                If ds.Tables("MPromosi").Rows.Count >= 1 Then
                    SQL = "UPDATE MLapOmzet SET " & vbCrLf & _
                          " PromosiPKP=" & FixKoma(NullToDbl(ds.Tables("MPromosi").Rows(0).Item("Total"))) & ", " & vbCrLf & _
                          " PPNPromosi=" & FixKoma(NullToDbl(ds.Tables("MPromosi").Rows(0).Item("PPN"))) & ", " & vbCrLf & _
                          " DPPPromosi=" & FixKoma(NullToDbl(ds.Tables("MPromosi").Rows(0).Item("DPP"))) & " " & vbCrLf & _
                          " WHERE " & where
                Else
                    SQL = "UPDATE MLapOmzet SET PromosiPKP=0, DPPPromosi=0, PPNPromosi=0 WHERE " & where
                End If
                com.CommandText = SQL
                com.ExecuteNonQuery()

                'Promosi Harian Non PKP
                SQL = "SELECT SUM(MJualD.Jumlah) AS Total " & vbCrLf & _
                      " FROM MJual (NOLOCK) " & vbCrLf & _
                      " INNER JOIN MJualD (NOLOCK) ON MJual.NoID=MJualD.IDJual " & vbCrLf & _
                      " INNER JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MJual.IDCustomer " & vbCrLf & _
                      " INNER JOIN MBarang (NOLOCK) ON MBarang.NoID=MJualD.IDBarang " & vbCrLf & _
                      " WHERE MBarang.IDKategori=28 AND IsNull(MAlamat.IsPKP,0)=0 AND MJual.Tanggal>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MJual.Tanggal<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "'"
                com.CommandText = SQL
                Hasil = NullToDbl(com.ExecuteScalar)
                Application.DoEvents()

                SQL = "UPDATE MLapOmzet SET PromosiNPKP=" & FixKoma(Hasil) & " WHERE " & where
                com.CommandText = SQL
                com.ExecuteNonQuery()

                'PPN Pembelian
                SQL = "SELECT SUM(MBeli.PPN) AS PPN" & _
                      " FROM MBeli (NOLOCK) " & _
                      " LEFT JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MBeli.IDSupplier" & _
                      " LEFT JOIN MAlamatDNPWP (NOLOCK) ON MAlamatDNPWP.NoID=MBeli.IDAlamatDNPWP" & vbCrLf & _
                      " WHERE IsNull(MBeli.IsTanpaBarang,0)=0 AND MBeli.IsTerimaFakturPajak=1 AND MBeli.MasaPajak>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MBeli.MasaPajak<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' AND (YEAR(MBeli.MasaPajak)=" & txtPeriode.DateTime.Year & " AND MONTH(MBeli.MasaPajak)=" & txtPeriode.DateTime.Month & ") "
                com.CommandText = SQL
                Hasil = NullToDbl(com.ExecuteScalar)
                Application.DoEvents()

                SQL = "UPDATE MLapOmzet SET FPPA=" & FixKoma(Hasil) & " WHERE " & where
                com.CommandText = SQL
                com.ExecuteNonQuery()

                'PPN Ongkos
                SQL = "SELECT SUM(MBeli.PPN) AS PPN" & _
                      " FROM MBeli (NOLOCK) " & _
                      " LEFT JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MBeli.IDSupplier" & _
                      " LEFT JOIN MAlamatDNPWP (NOLOCK) ON MAlamatDNPWP.NoID=MBeli.IDAlamatDNPWP" & vbCrLf & _
                      " WHERE IsNull(MBeli.IsTanpaBarang,0)=1 AND MBeli.IsTerimaFakturPajak=1 AND MBeli.MasaPajak>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MBeli.MasaPajak<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' AND (YEAR(MBeli.MasaPajak)=" & txtPeriode.DateTime.Year & " AND MONTH(MBeli.MasaPajak)=" & txtPeriode.DateTime.Month & ") "
                com.CommandText = SQL
                Hasil = NullToDbl(com.ExecuteScalar)
                Application.DoEvents()

                SQL = "UPDATE MLapOmzet SET FPOngkos=" & FixKoma(Hasil) & " WHERE " & where
                com.CommandText = SQL
                com.ExecuteNonQuery()

                'FP Retur Pembelian
                SQL = "SELECT SUM(MReturBeli.NilaiPPN) AS PPN " & _
                      " FROM MReturBeli (NOLOCK) " & _
                      " LEFT JOIN MAlamat (NOLOCK) ON MAlamat.NoID=MReturBeli.IDSupplier" & _
                      " LEFT JOIN MAlamatDNPWP (NOLOCK) ON MAlamatDNPWP.NoID=MReturBeli.IDAlamatDNPWP" & _
                      " WHERE MReturBeli.IsProsesPajak=1 AND MReturBeli.MasaPajak>='" & TglRun.ToString("yyyy-MM-dd") & "' AND MReturBeli.MasaPajak<'" & TglRun.AddDays(1).ToString("yyyy-MM-dd") & "' AND (YEAR(MReturBeli.MasaPajak)=" & txtPeriode.DateTime.Year & " AND MONTH(MReturBeli.MasaPajak)=" & txtPeriode.DateTime.Month & ") "
                com.CommandText = SQL
                Hasil = NullToDbl(com.ExecuteScalar)
                Application.DoEvents()

                SQL = "UPDATE MLapOmzet SET FPRetur=" & FixKoma(Hasil) & " WHERE " & where
                com.CommandText = SQL
                com.ExecuteNonQuery()

                ProgressBarControl1.Position = ((i + 1) / TglSampai.Day) * 100
                ProgressBarControl1.Refresh()
                Application.DoEvents()
            Next
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If
            If Not dlg Is Nothing Then
                dlg.Close()
            End If
            dlg.Dispose()
            ProgressBarControl1.Visible = False
            cn.Dispose()
            com.Dispose()
            oDA.Dispose()
            ds.Dispose()
        End Try
    End Sub

    Private Sub PanelControl2_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PanelControl2.Paint

    End Sub

    Private Sub ckEdit_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckEdit.CheckedChanged
        Tampilan()
    End Sub

    Private Sub Tampilan()
        If ckEdit.Checked Then
            GC1.Visible = True
            'SimpleButton8.PerformClick()
            CrViewer.Visible = False
        Else
            GC1.Visible = False
            CrViewer.Visible = True
        End If
    End Sub

    Private Sub RefreshData()
        Dim ds As New DataSet
        Try
            SQL = "SELECT MLapOmzet.*, DAY(MLapOmzet.Tanggal) AS [No], IsNull(MLapOmzet.Tunai,0)+IsNull(MLapOmzet.ObatPKP,0)+IsNull(MLapOmzet.ObatNPKP,0) AS [Z Report], MLapOmzet.Bank AS [C/C], IsNull(MLapOmzet.Bank,0)+IsNull(MLapOmzet.Setoran,0)-(IsNull(MLapOmzet.Tunai,0)+IsNull(MLapOmzet.ObatPKP,0)+IsNull(MLapOmzet.ObatNPKP,0)) AS SelisihKasir FROM MLapOmzet (NOLOCK) WHERE MONTH(MLapOmzet.Periode)=" & txtPeriode.DateTime.Month & " AND YEAR(MLapOmzet.Periode)=" & txtPeriode.DateTime.Year
            ds = ExecuteDataset("Data", SQL)
            GC1.DataSource = ds.Tables("Data")
            IsPeriodeLock = clsPostingPembelian.IsLockPeriodeFP(txtPeriode.DateTime)
            If System.IO.File.Exists(FolderLayouts & Me.Name & GV1.Name & ".xml") Then
                GV1.RestoreLayoutFromXml(FolderLayouts & Me.Name & GV1.Name & ".xml")
            End If
            GV1.OptionsDetail.SmartDetailExpandButtonMode = DetailExpandButtonMode.AlwaysEnabled
            For Each ctrl As GridView In GC1.Views
                With ctrl
                    For i As Integer = 0 To .Columns.Count - 1
                        Select Case .Columns(i).ColumnType.Name.ToLower
                            Case "int32", "int64", "int"
                                .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric
                                .Columns(i).DisplayFormat.FormatString = "n0"
                            Case "decimal", "single", "money", "double"
                                .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric
                                .Columns(i).DisplayFormat.FormatString = "n2"
                            Case "string"
                                .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.None
                                .Columns(i).DisplayFormat.FormatString = ""
                            Case "date", "datetime"
                                If .Columns(i).FieldName.Trim.ToLower = "jam" Then
                                    .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime
                                    .Columns(i).DisplayFormat.FormatString = "HH:mm"
                                ElseIf .Columns(i).FieldName.Trim.ToLower = "tanggalstart" Or .Columns(i).FieldName.Trim.ToLower = "tanggalend" Then
                                    .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime
                                    .Columns(i).DisplayFormat.FormatString = "dd-MM-yyyy HH:mm"
                                Else
                                    .Columns(i).DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime
                                    .Columns(i).DisplayFormat.FormatString = "dd-MM-yyyy"
                                End If
                            Case "byte[]"
                                reppicedit.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze
                                .Columns(i).OptionsColumn.AllowGroup = False
                                .Columns(i).OptionsColumn.AllowSort = False
                                .Columns(i).OptionsFilter.AllowFilter = False
                                .Columns(i).ColumnEdit = reppicedit
                            Case "boolean"
                                .Columns(i).ColumnEdit = repckedit
                        End Select
                    Next
                End With
            Next
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            ds.Dispose()
        End Try
    End Sub

    Private Sub SimpleButton8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRefresh.Click
        RefreshData()
        ckEdit.Checked = True
        Tampilan()
    End Sub

    Private Sub txtPeriode_EditValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPeriode.EditValueChanged
        cmdRefresh.PerformClick()
    End Sub

    Private Sub cmdHitungUlang_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdHitungUlang.Click
        Dim x As New frmOtorisasiAdmin
        If XtraMessageBox.Show("Yakin ingin menghitung ulang, data yang tersimpan akan kereset ke 0.", NamaAplikasi, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = Windows.Forms.DialogResult.Yes AndAlso x.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            HitungOmzetHarian(True, True)
            cmdRefresh.PerformClick()
        End If
        x.Dispose()
    End Sub

    Private Sub GV1_CellValueChanged(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs) Handles GV1.CellValueChanged
        Dim cn As New SqlConnection
        Dim com As New SqlCommand
        Dim i As Integer = 0, Hasil As Double = 0.0
        Try
            If ckEdit.Checked AndAlso (GV1.FocusedColumn.FieldName.ToUpper = "C/C".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "Setoran".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "RumusSetoran".ToUpper) Then
                cn.ConnectionString = StrKonSql
                cn.Open()
                com.Connection = cn
                Select Case GV1.FocusedColumn.FieldName.ToUpper
                    Case "RumusSetoran".ToUpper
                        Hasil = NullToDbl(Evaluate(Replace(e.Value.ToString, "=", "")))
                        com.CommandText = "UPDATE MLapOmzet SET RumusSetoran='" & FixApostropi(e.Value.ToString) & "', Setoran=" & FixKoma(Hasil) & " WHERE NoID=" & NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        com.ExecuteNonQuery()
                        i = NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        RefreshData()
                        GV1.ClearSelection()
                        GV1.FocusedRowHandle = GV1.LocateByDisplayText(0, GV1.Columns("NoID"), i.ToString("##0"))
                        GV1.SelectRow(GV1.FocusedRowHandle)
                        'GV1.FocusedColumn.FieldName.ToUpper = "[Z Report]".ToUpper Or 
                    Case "Z Report".ToUpper
                        com.CommandText = "UPDATE MLapOmzet SET Tunai=" & FixKoma(NullToDbl(e.Value)) & " WHERE NoID=" & NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        com.ExecuteNonQuery()
                        i = NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        RefreshData()
                        GV1.ClearSelection()
                        GV1.FocusedRowHandle = GV1.LocateByDisplayText(0, GV1.Columns("NoID"), i.ToString("##0"))
                        GV1.SelectRow(GV1.FocusedRowHandle)
                        'GV1.SetRowCellValue(e.RowHandle, "Bank", NullToDbl(e.Value))
                        'GV1.SetRowCellValue(e.RowHandle, "SelisihKasir", NullToDbl(e.Value) + NullToDbl(GV1.GetRowCellValue(e.RowHandle, "Setoran")) - NullToDbl(GV1.GetRowCellValue(e.RowHandle, "Tunai")))
                    Case "C/C".ToUpper
                        com.CommandText = "UPDATE MLapOmzet SET Bank=" & FixKoma(NullToDbl(e.Value)) & " WHERE NoID=" & NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        com.ExecuteNonQuery()
                        i = NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        RefreshData()
                        GV1.ClearSelection()
                        GV1.FocusedRowHandle = GV1.LocateByDisplayText(0, GV1.Columns("NoID"), i.ToString("##0"))
                        GV1.SelectRow(GV1.FocusedRowHandle)
                        'GV1.SetRowCellValue(e.RowHandle, "Bank", NullToDbl(e.Value))
                        'GV1.SetRowCellValue(e.RowHandle, "SelisihKasir", NullToDbl(e.Value) + NullToDbl(GV1.GetRowCellValue(e.RowHandle, "Setoran")) - NullToDbl(GV1.GetRowCellValue(e.RowHandle, "Tunai")))
                    Case "Setoran".ToUpper
                        com.CommandText = "UPDATE MLapOmzet SET RumusSetoran='" & FixApostropi(NullToLong(e.Value).ToString.Replace(".", "").Replace(",", "")) & "', Setoran=" & FixKoma(NullToDbl(e.Value)) & " WHERE NoID=" & NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        com.ExecuteNonQuery()
                        i = NullToLong(GV1.GetRowCellValue(e.RowHandle, "NoID"))
                        RefreshData()
                        GV1.ClearSelection()
                        GV1.FocusedRowHandle = GV1.LocateByDisplayText(0, GV1.Columns("NoID"), i.ToString("##0"))
                        GV1.SelectRow(GV1.FocusedRowHandle)
                        'GV1.SetRowCellValue(e.RowHandle, "SelisihKasir", NullToDbl(e.Value) + NullToDbl(GV1.GetRowCellValue(e.RowHandle, "C/C")) - NullToDbl(GV1.GetRowCellValue(e.RowHandle, "Tunai")))
                End Select
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        Finally
            If cn.State = ConnectionState.Open Then
                cn.Close()
            End If
            cn.Dispose()
            com.Dispose()
        End Try
    End Sub
    Private Sub GV1_FocusedColumnChanged(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Base.FocusedColumnChangedEventArgs) Handles GV1.FocusedColumnChanged
        If ckEdit.Checked AndAlso (GV1.FocusedColumn.FieldName.ToUpper = "RumusSetoran".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "C/C".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "Setoran".ToUpper) AndAlso Not IsPeriodeLock Then
            GV1.OptionsBehavior.Editable = True
        Else
            GV1.OptionsBehavior.Editable = False
        End If
    End Sub

    Private Sub GV1_FocusedRowChanged(ByVal sender As Object, ByVal e As DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs) Handles GV1.FocusedRowChanged
        If ckEdit.Checked AndAlso (GV1.FocusedColumn.FieldName.ToUpper = "RumusSetoran".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "C/C".ToUpper Or GV1.FocusedColumn.FieldName.ToUpper = "Setoran".ToUpper) AndAlso Not IsPeriodeLock Then
            GV1.OptionsBehavior.Editable = True
        Else
            GV1.OptionsBehavior.Editable = False
        End If
    End Sub

    Private Sub cmdFPSewaNPKP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPSewaNPKP.Click
        Dim NamaFile As String = Application.StartupPath & "\Report\PromosiBulanan.rpt"
        Try
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=1&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=False")
            Else
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=1&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=False")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private Sub cmdFPObatNPKP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdFPObatNPKP.Click
        Dim NamaFile As String = Application.StartupPath & "\Report\PromosiBulanan.rpt"
        Try
            If EditReport Then
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Edit, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=2&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=False")
            Else
                ViewReportLocal(Me.MdiParent, mdlCetakCR.action_.Preview, NamaFile, TryCast(sender, SimpleButton).Text, , , "Tipe=2&Periode=CDATE(" & txtPeriode.DateTime.ToString("yyyy,MM,01") & ")&IsPKP=False")
                ckEdit.Checked = False
                Tampilan()
            End If
        Catch ex As Exception
            XtraMessageBox.Show("Info Kesalahan : " & ex.Message, NamaAplikasi, MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub
End Class