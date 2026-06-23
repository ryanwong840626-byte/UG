Option Strict Off
Imports System
Imports System.IO
Imports NXOpen
Imports NXOpen.UF
Imports System.Globalization

Module SingleWorkpieceDualHeadCollisionModelV8
    Dim theSession As Session
    Dim theUfSession As UFSession
    Dim ci As CultureInfo = CultureInfo.InvariantCulture

    Const MoldRadius As Double = 40.0
    Const ChuckRadius As Double = 46.0
    Const FixtureBackRadius As Double = 34.0
    Const RoughWheelRadius As Double = 32.0
    Const RoughWheelThickness As Double = 8.0
    Const FinishWheelRadius As Double = 30.0
    Const FinishWheelThickness As Double = 6.0
    Const ShankRadius As Double = 6.0
    Const HeadZOffset As Double = 6.0

    Sub Main()
        theSession = Session.GetSession()
        theUfSession = UFSession.GetUFSession()

        Dim partTag As Tag
        Dim partName As String = "C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_workpiece_dual_head_collision_model_v8"
        theUfSession.Part.[New](partName, 2, partTag)

        CreateSceneAxes()
        CreateMoldAndFixture()
        CreateToolpathAndToolSnapshots("C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_station_samples.csv")

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

    Sub TryCreateCylinderZ(ByVal cx As Double, ByVal cy As Double, ByVal zMin As Double, ByVal height As Double, ByVal diameter As Double, ByVal color As Integer)
        TryCreateCylinderAlong(New Double() {cx, cy, zMin}, New Double() {0.0, 0.0, 1.0}, height, diameter, color)
    End Sub

    Sub CreateCircleXY(ByVal cx As Double, ByVal cy As Double, ByVal cz As Double, ByVal r As Double, ByVal color As Integer)
        Dim n As Integer = 96
        Dim prev() As Double = Nothing
        For i As Integer = 0 To n
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / CDbl(n)
            Dim p() As Double = {cx + Math.Cos(a) * r, cy + Math.Sin(a) * r, cz}
            If Not prev Is Nothing Then CreateLine(prev, p, color)
            prev = p
        Next
    End Sub

    Sub CreateCylinderWire(ByVal cx As Double, ByVal cy As Double, ByVal zMin As Double, ByVal zMax As Double, ByVal r As Double, ByVal color As Integer)
        CreateCircleXY(cx, cy, zMin, r, color)
        CreateCircleXY(cx, cy, zMax, r, color)
        For i As Integer = 0 To 7
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / 8.0
            Dim x As Double = cx + Math.Cos(a) * r
            Dim y As Double = cy + Math.Sin(a) * r
            CreateLine(New Double() {x, y, zMin}, New Double() {x, y, zMax}, color)
        Next
    End Sub

    Sub CreateMoldAndFixture()
        ' Mold/workpiece: clarified transparent mold diameter is 80 mm.
        TryCreateCylinderZ(0.0, 0.0, -3.0, 6.0, MoldRadius * 2.0, 36)
        TryCreateCylinderZ(0.0, 0.0, -17.0, 14.0, ChuckRadius * 2.0, 31)
        TryCreateCylinderZ(0.0, 0.0, -31.0, 14.0, FixtureBackRadius * 2.0, 8)
        CreateCylinderWire(0.0, 0.0, -3.0, 3.0, MoldRadius, 36)
        CreateCylinderWire(0.0, 0.0, -17.0, -3.0, ChuckRadius, 31)
        CreateCylinderWire(0.0, 0.0, -31.0, -17.0, FixtureBackRadius, 8)
        CreateLine(New Double() {0.0, 0.0, -35.0}, New Double() {0.0, 0.0, 18.0}, 211)
        CreateLine(New Double() {-MoldRadius, 0.0, 0.0}, New Double() {MoldRadius, 0.0, 0.0}, 36)
        CreateLine(New Double() {0.0, -MoldRadius, 0.0}, New Double() {0.0, MoldRadius, 0.0}, 36)
    End Sub

    Sub CreateSceneAxes()
        CreateLine(New Double() {-75.0, 0.0, 0.0}, New Double() {95.0, 0.0, 0.0}, 2)
        CreateLine(New Double() {0.0, -80.0, 0.0}, New Double() {0.0, 80.0, 0.0}, 3)
        CreateLine(New Double() {0.0, 0.0, -45.0}, New Double() {0.0, 0.0, 45.0}, 4)
    End Sub

    Sub CreateToolpathAndToolSnapshots(ByVal csvPath As String)
        If Not File.Exists(csvPath) Then
            Throw New Exception("Missing samples CSV: " + csvPath)
        End If

        Dim lines() As String = File.ReadAllLines(csvPath)
        Dim prevPass1() As Double = Nothing
        Dim prevPass2() As Double = Nothing
        Dim count1 As Integer = 0
        Dim count2 As Integer = 0

        For i As Integer = 1 To lines.Length - 1
            Dim s() As String = lines(i).Split(","c)
            If s.Length < 6 Then Continue For

            Dim xLocal As Double = Double.Parse(s(2), ci)
            Dim z As Double = Double.Parse(s(3), ci)
            Dim passId As Integer = Integer.Parse(s(5), ci)
            Dim p() As Double = {xLocal, 0.0, z + HeadZOffset}

            If passId = 1 Then
                count1 = count1 + 1
                If Not prevPass1 Is Nothing Then CreateLine(prevPass1, p, 186)
                If count1 = 1 Or count1 = 15 Or count1 = 29 Then
                    CreateToolSnapshot(xLocal, z + HeadZOffset, 1)
                End If
                prevPass1 = p
            Else
                count2 = count2 + 1
                If Not prevPass2 Is Nothing Then CreateLine(prevPass2, p, 31)
                If count2 = 1 Or count2 = 240 Or count2 = 480 Or count2 = 727 Then
                    CreateToolSnapshot(xLocal, z + HeadZOffset, 2)
                End If
                prevPass2 = p
            End If
        Next
    End Sub

    Sub CreateToolSnapshot(ByVal x As Double, ByVal z As Double, ByVal passId As Integer)
        Dim wheelRadius As Double = RoughWheelRadius
        Dim wheelThickness As Double = RoughWheelThickness
        Dim toolColor As Integer = 186
        Dim shankDir() As Double = {0.86, 0.0, 0.51}

        If passId = 2 Then
            wheelRadius = FinishWheelRadius
            wheelThickness = FinishWheelThickness
            toolColor = 211
            shankDir = New Double() {0.88, 0.0, 0.48}
        End If

        Dim y0 As Double = -wheelThickness / 2.0
        TryCreateCylinderAlong(New Double() {x, y0, z}, New Double() {0.0, 1.0, 0.0}, wheelThickness, wheelRadius * 2.0, toolColor)

        Dim shankStart() As Double = {x + wheelRadius * 0.55, 0.0, z + wheelRadius * 0.25}
        TryCreateCylinderAlong(shankStart, shankDir, 75.0, ShankRadius * 2.0, 8)

        CreateLine(New Double() {x - wheelRadius, 0.0, z}, New Double() {x + wheelRadius, 0.0, z}, toolColor)
        CreateLine(New Double() {x, 0.0, z - wheelRadius}, New Double() {x, 0.0, z + wheelRadius}, toolColor)
    End Sub

    Public Function GetUnloadOption(ByVal dummy As String) As Integer
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
End Module
