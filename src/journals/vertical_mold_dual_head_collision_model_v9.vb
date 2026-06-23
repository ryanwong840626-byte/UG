Option Strict Off
Imports System
Imports System.IO
Imports NXOpen
Imports NXOpen.UF
Imports System.Globalization

Module VerticalMoldDualHeadCollisionModelV9
    Dim theSession As Session
    Dim theUfSession As UFSession
    Dim ci As CultureInfo = CultureInfo.InvariantCulture

    Const MoldRadius As Double = 40.0
    Const MoldThickness As Double = 6.0
    Const ChuckRadius As Double = 46.0
    Const ChuckLength As Double = 20.0
    Const BackSpindleRadius As Double = 24.0
    Const BackSpindleLength As Double = 35.0
    Const RoughWheelRadius As Double = 32.0
    Const RoughWheelThickness As Double = 8.0
    Const FinishWheelRadius As Double = 30.0
    Const FinishWheelThickness As Double = 6.0
    Const ShankRadius As Double = 6.0
    Const ToolFaceX As Double = 8.0
    Const ToolZOffset As Double = 4.0

    Sub Main()
        theSession = Session.GetSession()
        theUfSession = UFSession.GetUFSession()

        Dim partTag As Tag
        Dim partName As String = "C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\vertical_mold_dual_head_collision_model_v9"
        theUfSession.Part.[New](partName, 2, partTag)

        CreateSceneAxes()
        CreateVerticalMoldAndChuck()
        CreateToolpathAndSnapshots("C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_station_samples.csv")

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

    Sub CreateVerticalMoldAndChuck()
        ' Real layout correction: mold is a vertical 80 mm disk held on a horizontal spindle.
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, MoldThickness, MoldRadius * 2.0, 36)
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0 - ChuckLength, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, ChuckLength, ChuckRadius * 2.0, 31)
        TryCreateCylinderAlong(New Double() {-MoldThickness / 2.0 - ChuckLength - BackSpindleLength, 0.0, 0.0}, New Double() {1.0, 0.0, 0.0}, BackSpindleLength, BackSpindleRadius * 2.0, 8)

        CreateCylinderWireX(-MoldThickness / 2.0, MoldThickness / 2.0, 0.0, 0.0, MoldRadius, 36)
        CreateCylinderWireX(-MoldThickness / 2.0 - ChuckLength, -MoldThickness / 2.0, 0.0, 0.0, ChuckRadius, 31)
        CreateCylinderWireX(-MoldThickness / 2.0 - ChuckLength - BackSpindleLength, -MoldThickness / 2.0 - ChuckLength, 0.0, 0.0, BackSpindleRadius, 8)

        CreateLine(New Double() {-70.0, 0.0, 0.0}, New Double() {20.0, 0.0, 0.0}, 2)
        CreateLine(New Double() {0.0, -MoldRadius, 0.0}, New Double() {0.0, MoldRadius, 0.0}, 36)
        CreateLine(New Double() {0.0, 0.0, -MoldRadius}, New Double() {0.0, 0.0, MoldRadius}, 36)
    End Sub

    Sub CreateSceneAxes()
        CreateLine(New Double() {-90.0, 0.0, 0.0}, New Double() {115.0, 0.0, 0.0}, 2)
        CreateLine(New Double() {0.0, -80.0, 0.0}, New Double() {0.0, 80.0, 0.0}, 3)
        CreateLine(New Double() {0.0, 0.0, -60.0}, New Double() {0.0, 0.0, 80.0}, 4)
    End Sub

    Sub CreateToolpathAndSnapshots(ByVal csvPath As String)
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

            Dim radiusOnMold As Double = Double.Parse(s(2), ci)
            Dim z As Double = Double.Parse(s(3), ci) + ToolZOffset
            Dim passId As Integer = Integer.Parse(s(5), ci)
            Dim p() As Double = {ToolFaceX, radiusOnMold, z}

            If passId = 1 Then
                count1 = count1 + 1
                If Not prevPass1 Is Nothing Then CreateLine(prevPass1, p, 186)
                If count1 = 1 Or count1 = 15 Or count1 = 29 Then
                    CreateToolSnapshot(radiusOnMold, z, 1)
                End If
                prevPass1 = p
            Else
                count2 = count2 + 1
                If Not prevPass2 Is Nothing Then CreateLine(prevPass2, p, 211)
                If count2 = 1 Or count2 = 240 Or count2 = 480 Or count2 = 727 Then
                    CreateToolSnapshot(radiusOnMold, z, 2)
                End If
                prevPass2 = p
            End If
        Next
    End Sub

    Sub CreateToolSnapshot(ByVal yOnMold As Double, ByVal zOnMold As Double, ByVal passId As Integer)
        Dim wheelRadius As Double = RoughWheelRadius
        Dim wheelThickness As Double = RoughWheelThickness
        Dim toolColor As Integer = 186
        Dim headOffsetY As Double = -8.0
        Dim headAxis() As Double = {0.62, 0.0, 0.78}

        If passId = 2 Then
            wheelRadius = FinishWheelRadius
            wheelThickness = FinishWheelThickness
            toolColor = 211
            headOffsetY = 8.0
            headAxis = New Double() {0.58, 0.0, 0.81}
        End If

        Dim center() As Double = {ToolFaceX + 8.0, yOnMold + headOffsetY, zOnMold + 12.0}
        Dim wheelOrigin() As Double = {
            center(0) - headAxis(0) * wheelThickness / 2.0,
            center(1) - headAxis(1) * wheelThickness / 2.0,
            center(2) - headAxis(2) * wheelThickness / 2.0
        }

        TryCreateCylinderAlong(wheelOrigin, headAxis, wheelThickness, wheelRadius * 2.0, toolColor)

        Dim shankStart() As Double = {
            center(0) + headAxis(0) * wheelThickness / 2.0,
            center(1) + headAxis(1) * wheelThickness / 2.0,
            center(2) + headAxis(2) * wheelThickness / 2.0
        }
        TryCreateCylinderAlong(shankStart, headAxis, 85.0, ShankRadius * 2.0, 8)

        CreateLine(New Double() {ToolFaceX, yOnMold, zOnMold}, center, toolColor)
    End Sub

    Public Function GetUnloadOption(ByVal dummy As String) As Integer
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
End Module
