Option Strict Off
Imports System
Imports System.IO
Imports NXOpen
Imports NXOpen.UF
Imports System.Globalization

Module MachineKinematicsXCZV13
    Dim theSession As Session
    Dim theUfSession As UFSession
    Dim ci As CultureInfo = CultureInfo.InvariantCulture

    ' Display coordinate mapping:
    ' NX X = machine X traverse of fixture/workpiece.
    ' NX Y = machine Z infeed/retract direction of grinding heads.
    ' NX Z = visual vertical direction on the mold face.
    ' C axis is drawn through the mold center, coaxial with the infeed direction.
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
    Const HeadBodyRadius As Double = 14.0
    Const HeadBodyLength As Double = 85.0
    Const WheelFaceY As Double = 48.0
    Const ContactY As Double = 3.0

    Sub Main()
        theSession = Session.GetSession()
        theUfSession = UFSession.GetUFSession()

        Dim partTag As Tag
        Dim partName As String = "C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\machine_kinematics_xcz_v13"
        theUfSession.Part.[New](partName, 2, partTag)

        CreateMachineAxes()
        CreateMovingFixtureAndMold()
        CreateTwoFixedGrindingHeads()
        CreateCenterlinePath("C:\Users\RyanW\Documents\Codex\2026-04-23-ug-cli-ide\assets\samples\single_station_samples.csv")

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

    Sub CreateCircleXZ(ByVal cx As Double, ByVal cy As Double, ByVal cz As Double, ByVal r As Double, ByVal color As Integer)
        Dim n As Integer = 128
        Dim prev() As Double = Nothing
        For i As Integer = 0 To n
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / CDbl(n)
            Dim p() As Double = {cx + Math.Cos(a) * r, cy, cz + Math.Sin(a) * r}
            If Not prev Is Nothing Then CreateLine(prev, p, color)
            prev = p
        Next
    End Sub

    Sub CreateCylinderWireY(ByVal x As Double, ByVal yMin As Double, ByVal yMax As Double, ByVal z As Double, ByVal r As Double, ByVal color As Integer)
        CreateCircleXZ(x, yMin, z, r, color)
        CreateCircleXZ(x, yMax, z, r, color)
        For i As Integer = 0 To 7
            Dim a As Double = 2.0 * Math.PI * CDbl(i) / 8.0
            Dim px As Double = x + Math.Cos(a) * r
            Dim pz As Double = z + Math.Sin(a) * r
            CreateLine(New Double() {px, yMin, pz}, New Double() {px, yMax, pz}, color)
        Next
    End Sub

    Sub CreateMachineAxes()
        ' X traverse rail and two head center marks.
        CreateLine(New Double() {-60.0, -55.0, -55.0}, New Double() {HeadCenterDistance + 80.0, -55.0, -55.0}, 2)
        CreateLine(New Double() {0.0, -68.0, -55.0}, New Double() {0.0, -42.0, -55.0}, 186)
        CreateLine(New Double() {HeadCenterDistance, -68.0, -55.0}, New Double() {HeadCenterDistance, -42.0, -55.0}, 211)

        ' C axis through mold center. Z infeed/retract is along the same displayed Y direction.
        CreateLine(New Double() {0.0, -70.0, 0.0}, New Double() {0.0, 90.0, 0.0}, 4)
        CreateLine(New Double() {HeadCenterDistance, -70.0, 0.0}, New Double() {HeadCenterDistance, 90.0, 0.0}, 4)
    End Sub

    Sub CreateMovingFixtureAndMold()
        ' Current displayed workpiece position is under the rough head. The X rail shows travel toward the finish head.
        TryCreateCylinderAlong(New Double() {0.0, -MoldThickness / 2.0, 0.0}, New Double() {0.0, 1.0, 0.0}, MoldThickness, MoldRadius * 2.0, 36)
        TryCreateCylinderAlong(New Double() {0.0, -MoldThickness / 2.0 - ChuckLength, 0.0}, New Double() {0.0, 1.0, 0.0}, ChuckLength, ChuckRadius * 2.0, 31)
        TryCreateCylinderAlong(New Double() {0.0, -MoldThickness / 2.0 - ChuckLength - SpindleLength, 0.0}, New Double() {0.0, 1.0, 0.0}, SpindleLength, SpindleRadius * 2.0, 8)

        CreateCylinderWireY(0.0, -MoldThickness / 2.0, MoldThickness / 2.0, 0.0, MoldRadius, 36)
        CreateCylinderWireY(0.0, -MoldThickness / 2.0 - ChuckLength, -MoldThickness / 2.0, 0.0, ChuckRadius, 31)
        CreateCylinderWireY(0.0, -MoldThickness / 2.0 - ChuckLength - SpindleLength, -MoldThickness / 2.0 - ChuckLength, 0.0, SpindleRadius, 8)

        ' Center horizontal contact line on the mold face.
        CreateLine(New Double() {-MoldRadius, ContactY, 0.0}, New Double() {MoldRadius, ContactY, 0.0}, 36)
    End Sub

    Sub CreateTwoFixedGrindingHeads()
        CreateHead(0.0, RoughWheelRadius, RoughWheelThickness, 186)
        CreateHead(HeadCenterDistance, FinishWheelRadius, FinishWheelThickness, 211)
    End Sub

    Sub CreateHead(ByVal headX As Double, ByVal wheelRadius As Double, ByVal wheelThickness As Double, ByVal color As Integer)
        ' Wheel face is outside the mold; it feeds along displayed Y/Z-infeed direction.
        TryCreateCylinderAlong(New Double() {headX, WheelFaceY, 0.0}, New Double() {0.0, 1.0, 0.0}, wheelThickness, wheelRadius * 2.0, color)
        TryCreateCylinderAlong(New Double() {headX, WheelFaceY + wheelThickness, 0.0}, New Double() {0.0, 1.0, 0.0}, HeadBodyLength, HeadBodyRadius * 2.0, 8)
        CreateLine(New Double() {headX, ContactY, 0.0}, New Double() {headX, WheelFaceY, 0.0}, color)
    End Sub

    Sub CreateCenterlinePath(ByVal csvPath As String)
        If Not File.Exists(csvPath) Then Exit Sub

        Dim lines() As String = File.ReadAllLines(csvPath)
        Dim prev1() As Double = Nothing
        Dim prev2() As Double = Nothing

        For i As Integer = 1 To lines.Length - 1
            Dim s() As String = lines(i).Split(","c)
            If s.Length < 6 Then Continue For

            Dim localX As Double = Double.Parse(s(2), ci)
            Dim infeedZ As Double = Double.Parse(s(3), ci)
            Dim passId As Integer = Integer.Parse(s(5), ci)

            ' Draw a thin kinematic curve near the mold centerline:
            ' horizontal coordinate comes from X traverse; vertical offset shows Z infeed variation.
            If passId = 1 Then
                Dim p1() As Double = {localX, ContactY + 2.0, infeedZ}
                If Not prev1 Is Nothing Then CreateLine(prev1, p1, 186)
                prev1 = p1
            Else
                Dim p2() As Double = {HeadCenterDistance + localX, ContactY + 2.0, infeedZ}
                If Not prev2 Is Nothing Then CreateLine(prev2, p2, 211)
                prev2 = p2
            End If
        Next
    End Sub

    Public Function GetUnloadOption(ByVal dummy As String) As Integer
        GetUnloadOption = NXOpen.Session.LibraryUnloadOption.Immediately
    End Function
End Module
