Option Strict Off
Imports System
Imports System.IO
Imports NXOpen
Imports NXOpen.UF
Imports System.Globalization

Module SingleStationCollisionModelV6
    Dim theSession As Session
    Dim theUfSession As UFSession
    Dim ci As CultureInfo = CultureInfo.InvariantCulture

    Const WorkpieceRadius As Double = 55.0
    Const FixtureRadius As Double = 45.0
    Const WheelRadius As Double = 34.0
    Const WheelThickness As Double = 10.0
    Const ShankRadius As Double = 4.0
    Const HeadZOffset As Double = 6.0

    Sub Main()
        theSession = Session.GetSession()
        theUfSession = UFSession.GetUFSession()

        Dim partTag As Tag
        Dim partName As String = "C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_station_collision_model_v6"
        theUfSession.Part.[New](partName, 2, partTag)

        CreateSceneAxes()
        CreateSingleStation()
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

    Sub CreateSingleStation()
        TryCreateCylinderZ(0.0, 0.0, -6.0, 12.0, WorkpieceRadius * 2.0, 36)
        TryCreateCylinderZ(0.0, 0.0, -28.0, 22.0, FixtureRadius * 2.0, 8)
        CreateCylinderWire(0.0, 0.0, -6.0, 6.0, WorkpieceRadius, 36)
        CreateCylinderWire(0.0, 0.0, -28.0, -6.0, FixtureRadius, 8)
        CreateLine(New Double() {0.0, 0.0, -35.0}, New Double() {0.0, 0.0, 18.0}, 211)
        CreateLine(New Double() {-WorkpieceRadius, 0.0, 0.0}, New Double() {WorkpieceRadius, 0.0, 0.0}, 36)
        CreateLine(New Double() {0.0, -WorkpieceRadius, 0.0}, New Double() {0.0, WorkpieceRadius, 0.0}, 36)
    End Sub

    Sub CreateSceneAxes()
        CreateLine(New Double() {-80.0, 0.0, 0.0}, New Double() {90.0, 0.0, 0.0}, 2)
        CreateLine(New Double() {0.0, -90.0, 0.0}, New Double() {0.0, 90.0, 0.0}, 3)
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
                If Not prevPass1 Is Nothing Then CreateLine(prevPass1, p, 36)
                If count1 = 1 Or count1 = 15 Or count1 = 29 Then
                    CreateToolSnapshot(xLocal, z + HeadZOffset)
                End If
                prevPass1 = p
            Else
                count2 = count2 + 1
                If Not prevPass2 Is Nothing Then CreateLine(prevPass2, p, 31)
                If count2 = 1 Or count2 = 240 Or count2 = 480 Or count2 = 727 Then
                    CreateToolSnapshot(xLocal, z + HeadZOffset)
                End If
                prevPass2 = p
            End If
        Next
    End Sub

    Sub CreateToolSnapshot(ByVal x As Double, ByVal z As Double)
        Dim y0 As Double = -WheelThickness / 2.0
        TryCreateCylinderAlong(New Double() {x, y0, z}, New Double() {0.0, 1.0, 0.0}, WheelThickness, WheelRadius * 2.0, 211)

        Dim shankStart() As Double = {x + WheelRadius * 0.55, 0.0, z + WheelRadius * 0.25}
        Dim shankDir() As Double = {0.94, 0.0, 0.34}
        TryCreateCylinderAlong(shankStart, shankDir, 72.0, ShankRadius * 2.0, 8)

        CreateLine(New Double() {x - WheelRadius, 0.0, z}, New Double() {x + WheelRadius, 0.0, z}, 211)
        CreateLine(New Double() {x, 0.0, z - WheelRadius}, New Double() {x, 0.0, z + WheelRadius}, 211)
    End Sub

    Public Function GetUnloadOption(ByVal dummy As String) As Integer
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
End Module
