Option Strict Off
Imports System
Imports System.IO
Imports NXOpen
Imports NXOpen.UF
Imports System.Globalization

Module MachineStructureDualHead197mmV12Clearance
    Dim theSession As Session
    Dim theUfSession As UFSession
    Dim ci As CultureInfo = CultureInfo.InvariantCulture

    Const MoldRadius As Double = 40.0
    Const MoldThickness As Double = 6.0
    Const ChuckRadius As Double = 46.0
    Const ChuckLength As Double = 22.0
    Const SpindleRadius As Double = 24.0
    Const SpindleLength As Double = 42.0
    Const HeadCenterDistance As Double = 197.0
    Const RoughWheelRadius As Double = 32.0
    Const RoughWheelThickness As Double = 8.0
    Const FinishWheelRadius As Double = 30.0
    Const FinishWheelThickness As Double = 6.0
    Const ToolBodyRadius As Double = 14.0
    Const ToolBodyLength As Double = 90.0
    Const ToolFaceX As Double = 42.0
    Const ToolZOffset As Double = 4.0

    Sub Main()
        theSession = Session.GetSession()
        theUfSession = UFSession.GetUFSession()

        Dim partTag As Tag
        Dim partName As String = "C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\machine_structure_dual_head_197mm_v12_clearance"
        theUfSession.Part.[New](partName, 2, partTag)

        CreateSceneAxes()
        CreateMoldChuckAndSpindle()
        CreateTwoFixedHeads197()
        CreatePassCurves("C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_station_samples.csv")

        theUfSession.Part.Save()
    End Sub

    Sub CreateLine(ByVal p1() As Double, ByVal p2() As Double, ByVal color As Integer)
        Dim lineTag As Tag
        Dim lineData As UFCurve.Line = New UFCurve.Line()
        lineData.start_point = p1
        lineData.end_point = p2
        theUfSession.Curve.CreateLine(lineData, lineTag)
        theUfSession.Obj.SetColor(lineTag, color)
    End Sub

    Sub TryCreateCylinderAlong(ByVal origin() As Double, ByVal direction() As Double, ByVal height As Double, ByVal diameter As Double, ByVal color As Integer)
        Try
            Dim featureTag As Tag
            theUfSession.Modl.CreateCylinder(FeatureSigns.Nullsign, Tag.Null, origin, height.ToString(ci), diameter.ToString(ci), direction, featureTag)
            theUfSession.Obj.SetColor(featureTag, color)
        Catch ex As Exception
            Dim p2() As Double = {origin(0) + direction(0) * height, origin(1) + direction(1) * height, origin(2) + direction(2) * height}
            CreateLine(origin, p2, color)
        End Try
    End Sub

    Sub CreateCircleYZ(ByVal cx As Double, ByVal cy As Double, ByVal cz As Double, ByVal r As Double, ByVal color As Integer)
        Dim n As Integer = 128
        Dim prev() As Double = Nothing
        For i As Integer = 0 To n
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / CDbl(n)
            Dim p() As Double = {cx, cy + Math.Cos(a) * r, cz + Math.Sin(a) * r}
            If Not prev Is Nothing Then CreateLine(prev, p, color)
            prev = p
        Next
    End Sub

    Sub CreateCylinderWireX(ByVal xMin As Double, ByVal xMax As Double, ByVal cy As Double, ByVal cz As Double, ByVal r As Double, ByVal color As Integer)
        CreateCircleYZ(xMin, cy, cz, r, color)
        CreateCircleYZ(xMax, cy, cz, r, color)
        For i As Integer = 0 To 7
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / 8.0
            Dim y As Double = cy + Math.Cos(a) * r
            Dim z As Double = cz + Math.Sin(a) * r
            CreateLine(New Double() {xMin, y, z}, New Double() {xMax, y, z}, color)
        Next
    End Sub

    Sub CreateSceneAxes()
        CreateLine(New Double() {-95.0, -30.0, 0.0}, New Double() {125.0, -30.0, 0.0}, 2)
        CreateLine(New Double() {0.0, -70.0, 0.0}, New Double() {0.0, HeadCenterDistance + 70.0, 0.0}, 3)
        CreateLine(New Double() {0.0, -30.0, -60.0}, New Double() {0.0, -30.0, 90.0}, 4)
        CreateLine(New Double() {ToolFaceX, 0.0, 0.0}, New Double() {ToolFaceX, HeadCenterDistance, 0.0}, 186)
    End Sub

    Sub CreateMoldChuckAndSpindle()
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, MoldThickness, MoldRadius * 2.0, 36)
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0 - ChuckLength, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, ChuckLength, ChuckRadius * 2.0, 31)
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0 - ChuckLength - SpindleLength, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, SpindleLength, SpindleRadius * 2.0, 8)

        CreateCylinderWireX(-MoldThickness / 2.0, MoldThickness / 2.0, 0.0, 0.0, MoldRadius, 36)
        CreateCylinderWireX(-MoldThickness / 2.0 - ChuckLength, -MoldThickness / 2.0, 0.0, 0.0, ChuckRadius, 31)
        CreateCylinderWireX(-MoldThickness / 2.0 - ChuckLength - SpindleLength, -MoldThickness / 2.0 - ChuckLength, 0.0, 0.0, SpindleRadius, 8)
    End Sub

    Sub CreateTwoFixedHeads197()
        CreateHead(0.0, 18.0, RoughWheelRadius, RoughWheelThickness, 186)
        CreateHead(HeadCenterDistance, 26.0, FinishWheelRadius, FinishWheelThickness, 211)
    End Sub

    Sub CreateHead(ByVal headY As Double, ByVal zOffset As Double, ByVal wheelRadius As Double, ByVal wheelThickness As Double, ByVal color As Integer)
        Dim axis() As Double = {0.58, 0.0, 0.81}
        Dim wheelCenter() As Double = {ToolFaceX + 8.0, headY, zOffset}
        Dim wheelOrigin() As Double = {
            wheelCenter(0) - axis(0) * wheelThickness / 2.0,
            wheelCenter(1) - axis(1) * wheelThickness / 2.0,
            wheelCenter(2) - axis(2) * wheelThickness / 2.0
        }

        TryCreateCylinderAlong(wheelOrigin, axis, wheelThickness, wheelRadius * 2.0, color)

        Dim bodyOrigin() As Double = {
            wheelCenter(0) + axis(0) * wheelThickness / 2.0,
            wheelCenter(1) + axis(1) * wheelThickness / 2.0,
            wheelCenter(2) + axis(2) * wheelThickness / 2.0
        }
        TryCreateCylinderAlong(bodyOrigin, axis, ToolBodyLength, ToolBodyRadius * 2.0, 8)
    End Sub

    Sub CreatePassCurves(ByVal csvPath As String)
        If Not File.Exists(csvPath) Then
            Throw New Exception("Missing samples CSV: " + csvPath)
        End If

        Dim lines() As String = File.ReadAllLines(csvPath)
        Dim prevPass1() As Double = Nothing
        Dim prevPass2() As Double = Nothing

        For i As Integer = 1 To lines.Length - 1
            Dim s() As String = lines(i).Split(","c)
            If s.Length < 6 Then Continue For

            Dim localRadius As Double = Double.Parse(s(2), ci)
            Dim z As Double = Double.Parse(s(3), ci) + ToolZOffset
            Dim passId As Integer = Integer.Parse(s(5), ci)

            If passId = 1 Then
                Dim p1() As Double = {ToolFaceX, localRadius, z}
                If Not prevPass1 Is Nothing Then CreateLine(prevPass1, p1, 186)
                prevPass1 = p1
            Else
                Dim p2() As Double = {ToolFaceX, HeadCenterDistance + localRadius, z}
                If Not prevPass2 Is Nothing Then CreateLine(prevPass2, p2, 211)
                prevPass2 = p2
            End If
        Next
    End Sub

    Public Function GetUnloadOption(ByVal dummy As String) As Integer
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
End Module
