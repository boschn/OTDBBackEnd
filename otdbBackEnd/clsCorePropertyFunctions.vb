
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE PropertyFunctions Classes for On Track Database Backend Library
REM *********** A Property function is a property with parameters in the form "PROP(ARG1, ARG2, ... )" or "PROP"
REM *********** which will be translated from String to a data structure with enumeration and vice versa
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-06
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2014
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Data
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports System.Attribute
Imports System.IO
Imports System.Text.RegularExpressions

Imports OnTrack.UI
Imports System.Reflection

Namespace OnTrack.Database


    ''' <summary>
    ''' PropertyFunction base Property Class
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class AbstractPropertyFunction(Of T)


        Protected _property As T
        Protected _arguments As Object()

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="property"></param>
        ''' <remarks></remarks>
        Public Sub New([property] As T)
            _property = [property]
        End Sub

        ''' <summary>
        ''' Constructor with arguments
        ''' </summary>
        ''' <param name="property"></param>
        ''' <param name="arguments"></param>
        ''' <remarks></remarks>
        Public Sub New([property] As T, ByVal ParamArray arguments() As Object)
            _property = [property]
            _arguments = arguments
        End Sub
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            Dim aName As String
            Dim arguments As String()
            '** extract arguments
            If propertystring.Contains("(") Then
                aName = propertystring.Substring(0, propertystring.IndexOf("("c)).ToUpper 'length
                Dim i = propertystring.LastIndexOf(")"c)
                If i > 0 Then
                    arguments = propertystring.Substring(propertystring.IndexOf("("c) + 1, i - 1 - propertystring.IndexOf("("c)).Split(","c)
                Else
                    arguments = propertystring.Substring(propertystring.IndexOf("("c) + 1).Split(","c)
                End If

                Dim aList As New List(Of String)
                For Each arg In arguments
                    aList.Add(arg.ToUpper.Trim)
                Next
                _arguments = aList.ToArray
            Else
                aName = propertystring
            End If
            _property = ToEnum(aName)
        End Sub
        ''' <summary>
        ''' set the enumeration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property [Enum] As T
            Get
                Return _property
            End Get

        End Property
        ''' <summary>
        ''' set or gets the arguments
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Arguments As Object()
            Get
                Return _arguments
            End Get

        End Property

        ''' <summary>
        ''' String representation of this Property
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToString() As String
            Dim aString As String = MyClass.ToString(_property).ToUpper
            If _arguments IsNot Nothing AndAlso _arguments.Count > 0 Then
                aString &= "("
                For i = 0 To _arguments.Count - 1
                    If i > 0 Then aString &= ","
                    aString &= _arguments(i)
                Next
                aString &= ")"
            End If
            Return aString
        End Function
        ''' <summary>
        ''' retuns the enumeration of a string presentation
        ''' </summary>
        ''' <param name="property"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToEnum(ByVal [property] As String) As T
            Dim fieldinfo() As FieldInfo = GetType(T).GetFields

            ' Loop over the fields.
            For Each field As FieldInfo In fieldinfo
                ' See if this is a literal value
                ' (set at compile time).
                If field.IsLiteral Then
                    If [property].ToUpper = field.Name.ToUpper Then
                        Return CType(field.GetValue(Nothing), T)
                    Else
                        ' List it.
                        For Each attribute In field.GetCustomAttributes(True)
                            If attribute.GetType.Equals(GetType(DescriptionAttribute)) Then
                                If [property].ToUpper = DirectCast(attribute, DescriptionAttribute).Description.ToUpper Then
                                    Return CType(field.GetValue(Nothing), T)
                                End If
                            End If
                        Next
                    End If


                End If
            Next field

            '** throw error
            Throw New Exception(message:="enumeration of " & GetType(T).Name & " has not the defined '" & [property] & "'")


        End Function
        ''' <summary>
        ''' validates the property string against the enumeration T
        ''' </summary>
        ''' <param name="property"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Validate(Of T)(ByVal [property] As String) As Boolean
            Dim fieldinfo() As FieldInfo = GetType(T).GetFields

            ' Loop over the fields.
            For Each field As FieldInfo In fieldinfo
                ' See if this is a literal value
                ' (set at compile time).
                If field.IsLiteral Then
                    If [property].ToUpper = field.Name.ToUpper Then
                        Return True
                    Else
                        ' List it.
                        For Each attribute In field.GetCustomAttributes(True)
                            If attribute.GetType.Equals(GetType(DescriptionAttribute)) Then
                                If [property].ToUpper = DirectCast(attribute, DescriptionAttribute).Description.ToUpper Then
                                    Return True
                                End If
                            End If
                        Next
                    End If


                End If
            Next field
            Return False
        End Function
        ''' <summary>
        ''' returns the string presentation of the enum 
        ''' </summary>
        ''' <param name="enumconstant"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ToString(ByVal enumconstant As T) As String
            Dim fi As Reflection.FieldInfo = enumconstant.GetType().GetField(enumconstant.ToString())
            Dim aattr() As DescriptionAttribute = DirectCast(fi.GetCustomAttributes(GetType(DescriptionAttribute), False), DescriptionAttribute())
            If aattr.Length > 0 Then
                Return aattr(0).Description
            Else
                Return enumconstant.ToString()
            End If
        End Function
    End Class

    ''' <summary>
    ''' ObjectPermission Rule Property
    ''' 
    ''' </summary>
    ''' <remarks> 
    ''' Validation Rules like 
    ''' 1) OTDBACCESS( DBACCESSRIGHT, FALSE|TRUE, FALSE|TRUE) which checks if the user has the DB Access right, then return ARG2 
    ''' and end the permission checking if third argument is true
    ''' 2) GROUP( [GROUPNAME] FALSE|TRUE, FALSE|TRUE) which checks if the user is in the group by name, then return ARG2 
    ''' and end the permission checking if third argument is true
    ''' 3) USER ( [USERNAME], FALSE|TRUE, FALSE|TRUE) which checks if the user is the username, then return ARG2 
    ''' and end the permission checking if third argument is true
    ''' </remarks>
    Public Class ObjectPermissionRuleProperty
        Inherits AbstractPropertyFunction(Of otObjectPermissionRuleProperty)
        Public Const DBAccess = "OTDBACCESS"
        Public Const Group = "GROUP"
        Public Const UserID = "USER"

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
            If Not Validate(Me) Then
                CoreMessageHandler(message:="Argument value is not valid", arg1:=propertystring, subname:="ObjectPermissionRuleProperty.New", _
                                    messagetype:=otCoreMessageType.InternalError)
            End If
        End Sub
        ''' <summary>
        ''' returns True if ExitOnTrue Flag is set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ExitOnTrue
            Get
                If Not Validate() Then Return False

                Select Case _property
                    Case otObjectPermissionRuleProperty.DBAccess
                        Return CBool(_arguments(1))
                    Case otObjectPermissionRuleProperty.Group, otObjectPermissionRuleProperty.User
                        Return CBool(_arguments(0))
                    Case Else
                        Return True
                End Select
            End Get
        End Property
        ''' <summary>
        ''' returns True if ExitOnTrue Flag is set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ExitOnFalse
            Get
                If Not Validate() Then Return False

                Select Case _property
                    Case otObjectPermissionRuleProperty.DBAccess
                        Return CBool(_arguments(2))
                    Case otObjectPermissionRuleProperty.Group, otObjectPermissionRuleProperty.User
                        Return CBool(_arguments(1))
                    Case Else
                        Return True
                End Select
            End Get
        End Property
        ''' <summary>
        ''' validates the property
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Validate() As Boolean
            Return Validate(Me)
        End Function
        ''' <summary>
        ''' validates the property
        ''' </summary>
        ''' <param name="property"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Validate([property] As ObjectPermissionRuleProperty) As Boolean
            Try
                Select Case [property].Enum
                    Case otObjectPermissionRuleProperty.DBAccess
                        If [property].Arguments.Count = 3 Then
                            Dim accessright As AccessRightProperty = New AccessRightProperty([property].Arguments(0).ToString)
                            If Not CBool([property].Arguments(1)) Then
                                CoreMessageHandler(message:="second argument must be a bool ", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                                Return False
                            End If
                            If Not CBool([property].Arguments(2)) Then
                                CoreMessageHandler(message:="third argument must be a bool ", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                                Return False
                            End If
                            Return True
                        Else
                            CoreMessageHandler(message:="Number of arguments wrong (should be 3)", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                            Return False
                        End If
                    Case otObjectPermissionRuleProperty.Group, otObjectPermissionRuleProperty.User
                        If [property].Arguments.Count = 1 Then
                            If Not CBool([property].Arguments(0)) Then
                                CoreMessageHandler(message:="first argument must be a bool ", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                                Return False
                            End If
                            If Not CBool([property].Arguments(1)) Then
                                CoreMessageHandler(message:="second argument must be a bool ", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                                Return False
                            End If
                            Return True
                        Else
                            CoreMessageHandler(message:="Number of arguments wrong (should be one)", arg1:=[property].ToString, _
                                               subname:="ObjectpermissionRuleProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                            Return False
                        End If
                    Case Else
                        Return True
                End Select
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectpermissionRuleProperty.Validate")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otObjectPermissionRuleProperty
            Return AbstractPropertyFunction(Of otObjectPermissionRuleProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otObjectPermissionRuleProperty
        <Description(ObjectPermissionRuleProperty.DBAccess)> DBAccess
        <Description(ObjectPermissionRuleProperty.Group)> Group
        <Description(ObjectPermissionRuleProperty.UserID)> User
    End Enum

    ''' <summary>
    ''' ForeignKey Property
    ''' 
    ''' </summary>
    ''' <remarks> 
    ''' Validation Rules like 
    ''' 1) ONDELETE( CASCADE | RESTRICT | DEFAULT | NULL | NOOP ) which checks if the user has the DB Access right, then return ARG2 
    ''' and end the permission checking if third argument is true
    ''' 2) ONUPDATE (CASCADE | RESTRICT | DEFAULT | NULL | NOOP ) which checks if the user is in the group by name, then return ARG2 
    ''' and end the permission checking if third argument is true
    ''' </remarks>
    Public Class ForeignKeyProperty
        Inherits AbstractPropertyFunction(Of otForeignKeyProperty)
        Public Const OnUpdate = "ONUPDATE"
        Public Const OnDelete = "ONDELETE"

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
            If Not Validate(Me) Then
                CoreMessageHandler(message:="Argument value is not valid", arg1:=propertystring, subname:="ObjectPermissionRuleProperty.New", _
                                    messagetype:=otCoreMessageType.InternalError)
            End If
        End Sub

        ''' <summary>
        ''' returns the ForeignKey Action Property
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ActionProperty() As ForeignKeyActionProperty
            Return New ForeignKeyActionProperty(Me.Arguments(0).ToString)
        End Function
        ''' <summary>
        ''' returns the Foreign Key Action enumeration
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Action() As otForeignKeyAction
            Return New ForeignKeyActionProperty(Me.Arguments(0).ToString).ToEnum
        End Function
        ''' <summary>
        ''' validates the property
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Validate() As Boolean
            Return Validate(Me)
        End Function
        ''' <summary>
        ''' validates the property
        ''' </summary>
        ''' <param name="property"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Validate([property] As ForeignKeyProperty) As Boolean
            Try
                Select Case [property].Enum
                    Case otForeignKeyProperty.OnUpdate, otForeignKeyProperty.OnDelete
                        If [property].Arguments.Count = 1 Then
                            If Not ForeignKeyActionProperty.Validate([property].Arguments(0).ToString) Then
                                CoreMessageHandler(message:="argument must be of otForeignKeyAction ", arg1:=[property].ToString, _
                                               subname:="ForeignKeyProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                                Return False
                            End If
                            Return True
                        Else
                            CoreMessageHandler(message:="Number of arguments wrong (should be 1)", arg1:=[property].ToString, _
                                               subname:="ForeignKeyProperty.Validate", messagetype:=otCoreMessageType.InternalError)
                            Return False
                        End If
                    Case Else
                        Return False
                End Select
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ForeignKeyProperty.Validate")
                Return False
            End Try

        End Function
        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function ToEnum() As otForeignKeyProperty
            Return AbstractPropertyFunction(Of otForeignKeyProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otForeignKeyProperty
        <Description(ForeignKeyProperty.OnUpdate)> OnUpdate = 1
        <Description(ForeignKeyProperty.OnDelete)> OnDelete
    End Enum
    ''' <summary>
    ''' ObjectPermission Rule Property
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ForeignKeyActionProperty
        Inherits AbstractPropertyFunction(Of otForeignKeyAction)

        Public Const Cascade = "CASCADE"
        Public Const NOOP = "NOOP"
        Public Const Restrict = "RESTRICT"
        Public Const SetDefault = "DEFAULT"
        Public Const SetNull = "NULL"

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
        End Sub

        ''' <summary>
        ''' Validate the string before a Property is created
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Validate(propertystring As String) As Boolean
            Return AbstractPropertyFunction(Of ForeignKeyActionProperty).Validate(Of otForeignKeyAction)(propertystring)
        End Function

        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otForeignKeyAction
            Return AbstractPropertyFunction(Of otForeignKeyAction).ToEnum(_property)
        End Function

    End Class
    ''' <summary>
    ''' Enumeration for Access Rights to the database
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otForeignKeyAction
        <Description(ForeignKeyActionProperty.Cascade)> Cascade = 0
        <Description(ForeignKeyActionProperty.NOOP)> Noop
        <Description(ForeignKeyActionProperty.Restrict)> Restrict
        <Description(ForeignKeyActionProperty.SetNull)> SetNull
        <Description(ForeignKeyActionProperty.SetDefault)> SetDefault
    End Enum


    ''' <summary>
    ''' ObjectEntry (Field) Property Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ObjectEntryProperty
        Inherits AbstractPropertyFunction(Of otObjectEntryProperty)
        Public Const Upper = "UPPER"
        Public Const Lower = "LOWER"
        Public Const Trim = "TRIM"
        Public Const Capitalize = "CAPITALIZE"
        Public Const Keyword = "KEYWORD"
        Public Const Encrypted = "ENCRYPTED"
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
        End Sub
        ''' <summary>
        ''' Apply the Property function to a value
        ''' </summary>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Apply(ByVal [in] As String(), ByRef [out] As String()) As Boolean
            If [in] Is Nothing Then Return True
            For i = 0 To [in].Count - 1
                Me.Apply([in]:=[in](i), out:=out(i))
            Next
            Return True
        End Function
        ''' <summary>
        ''' Apply the Property function to a value
        ''' </summary>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Apply(ByVal [in] As Object, ByRef [out] As Object) As Boolean
            If [in] Is Nothing Then
                [out] = [in]
                Return True
            End If
            Select Case _property
                Case otObjectEntryProperty.Lower
                    [out] = [in].ToString.ToLower
                    Return True
                Case otObjectEntryProperty.Upper
                    [out] = [in].ToString.ToUpper
                    Return True
                Case otObjectEntryProperty.Trim
                    [out] = [in].ToString.Trim
                    Return True
                Case otObjectEntryProperty.Keyword
                    [out] = [in].ToString.Trim.ToUpper
                    Return True
                Case otObjectEntryProperty.Capitalize
                    [out] = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase([in].ToString)
                    Return True
                Case otObjectEntryProperty.Encrypted
                    [out] = [in].ToString.Trim
                    Return True
                Case Else
                    CoreMessageHandler(message:="Property function is not implemented", arg1:=_property.ToString, messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectEntryProperty.Apply")
                    Return False
            End Select
        End Function
        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otObjectEntryProperty
            Return AbstractPropertyFunction(Of otObjectEntryProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otObjectEntryProperty
        <Description(ObjectEntryProperty.Upper)> Upper
        <Description(ObjectEntryProperty.Lower)> Lower
        <Description(ObjectEntryProperty.Trim)> Trim
        <Description(ObjectEntryProperty.Capitalize)> Capitalize
        <Description(ObjectEntryProperty.Keyword)> Keyword
        <Description(ObjectEntryProperty.Encrypted)> Encrypted
    End Enum


    ''' <summary>
    ''' ObjectEntry Validation Property Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class LookupProperty
        Inherits AbstractPropertyFunction(Of otLookupProperty)
        Public Const UseAttributeReference = "USEREFERENCE"
        Public Const UseAttributeValues = "USEATTRIBUTEVALUES"
        Public Const UseForeignKey = "USEFOREIGNKEY"
        Public Const UseObject = "USEOBJECT"
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
        End Sub

        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otLookupProperty
            Return AbstractPropertyFunction(Of otLookupProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otLookupProperty
        <Description(LookupProperty.UseAttributeReference)> UseAttributeReference = 1
        <Description(LookupProperty.UseForeignKey)> UseForeignKey
        <Description(LookupProperty.UseObject)> UseObject
        <Description(LookupProperty.UseAttributeValues)> UseAttributeValues

    End Enum


    ''' <summary>
    ''' Render Property Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RenderProperty
        Inherits AbstractPropertyFunction(Of otRenderProperty)
        Public Const PASSWORD = "PASSWORD"

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
        End Sub

        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otRenderProperty
            Return AbstractPropertyFunction(Of otRenderProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otRenderProperty
        <Description(RenderProperty.PASSWORD)> Password

    End Enum
    '*************************************************************************************
    '*************************************************************************************
    ''' <summary>
    ''' ObjectPermission Rule Property
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AccessRightProperty
        Inherits AbstractPropertyFunction(Of otAccessRight)
        '*** ACCESS RIGHTS CONSTANTS
        Public Const ConstARReadonly = "READONLY"
        Public Const ConstARReadUpdate = "READUPDATE"
        Public Const ConstARAlter = "ALTERSCHEMA"
        Public Const ConstARProhibited = "PROHIBITED"

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="propertystring"></param>
        ''' <remarks></remarks>
        Public Sub New(propertystring As String)
            MyBase.New(propertystring:=propertystring)
        End Sub

        Public Sub New([enum] As otAccessRight)
            MyBase.New(property:=[enum])
        End Sub

        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otAccessRight
            Return AbstractPropertyFunction(Of otAccessRight).ToEnum(_property)
        End Function
        ''' <summary>
        ''' Returns a List of Higher Access Rights then the one selected
        ''' </summary>
        ''' <param name="accessrequest"></param>
        ''' <param name="domain" >Domain to validate for</param>
        ''' <param name="Objects" >list of Obejectnames to validate in the domain</param>
        ''' <returns>eturns false if reverification of User is needed or true if currentAccessLevel includes this new request Level</returns>
        ''' <remarks></remarks>

        Public Shared Function GetHigherAccessRequests(ByVal accessrequest As otAccessRight) As List(Of String)

            Dim aResult As New List(Of String)

            If accessrequest = otAccessRight.AlterSchema Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
            End If

            If accessrequest = otAccessRight.ReadUpdateData Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
                aResult.Add(otAccessRight.ReadUpdateData.ToString)
            End If

            If accessrequest = otAccessRight.ReadOnly Then
                aResult.Add(otAccessRight.AlterSchema.ToString)
                aResult.Add(otAccessRight.ReadUpdateData.ToString)
                aResult.Add(otAccessRight.ReadOnly.ToString)
            End If

            Return aResult
        End Function
        ''' <summary>
        ''' shared version of coverrights of who to cover
        ''' </summary>
        ''' <param name="who"></param>
        ''' <param name="covers"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CoverRights(who As AccessRightProperty, covers As AccessRightProperty)
            Return who.CoverRights(covers)
        End Function
        ''' <summary>
        ''' returns true if the accessrightproperty (as request) is covered by this access right
        ''' </summary>
        ''' <param name="accessrightpropery"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CoverRights([accessrightpropery] As AccessRightProperty) As Boolean
            Return CoverRights([accessrightpropery].[Enum])
        End Function
        ''' <summary>
        ''' cover rights and what to cover
        ''' </summary>
        ''' <param name="rights"></param>
        ''' <param name="covers"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CoverRights(rights As otAccessRight, covers As otAccessRight) As Boolean

            If rights = covers Then
                Return True
            ElseIf covers = otAccessRight.[ReadOnly] And (rights = otAccessRight.ReadUpdateData Or rights = otAccessRight.AlterSchema) Then
                Return True
            ElseIf covers = otAccessRight.ReadUpdateData And rights = otAccessRight.AlterSchema Then
                Return True
                ' will never be reached !
            ElseIf covers = otAccessRight.AlterSchema And rights = otAccessRight.AlterSchema Then
                Return True
            Else
                Return False
            End If

        End Function
        ''' <summary>
        ''' returns true if the accessrequest  is covered by this access right
        ''' </summary>
        ''' <param name="accessrightpropery"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CoverRights(accessrequest As otAccessRight) As Boolean
            Return CoverRights(rights:=Me.[Enum], covers:=accessrequest)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration for Access Rights to the database
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otAccessRight
        <Description(AccessRightProperty.ConstARProhibited)> Prohibited = 0
        <Description(AccessRightProperty.ConstARReadonly)> [ReadOnly] = 1
        <Description(AccessRightProperty.ConstARReadUpdate)> ReadUpdateData = 2
        <Description(AccessRightProperty.ConstARAlter)> AlterSchema = 4
    End Enum


End Namespace