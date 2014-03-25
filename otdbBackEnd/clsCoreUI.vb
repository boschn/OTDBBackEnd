REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** User Interface Mapper Classes - These Classes provide UI independent thin mapping to an UI (WinForms, Telerik, WPF)
REM *********** 
REM *********** For Adding new Classes and UIs:
REM ***********
REM ***********  - Add a Const in the OTDBUI
REM ***********  - Add a Interface for the UI Class to implement
REM ***********
REM ***********  - Register the Mapping in the Startup somewhere !
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-09-13
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug


Imports OnTrack
Imports OnTrack.Database


Namespace OnTrack.UI



    ''' <summary>
    ''' base interface of the native Forms to fullfill
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iUINativeForm
        ''' <summary>
        ''' Connect with OTDB counterpart
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property OtdbShadow As iOTDBUIAbstractForm ' for call back to the OTDB UI Form
        ''' <summary>
        ''' show the native Form
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function ShowOTDBForm()
        ''' <summary>
        ''' Close the Form
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function CloseOTDBForm()
        ''' <summary>
        ''' Refresh the Form
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function RefreshOTDBForm()
    End Interface



    ''' <summary>
    ''' Interface of the MessageBox Form the mapped UI class has to fullfill
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iUINativeFormMessageBox
        Inherits iUINativeForm


        Property Message As String
        Property [Buttons] As CoreMessageBox.ButtonType
        Property Title As String
        Property Result As CoreMessageBox.ResultType
        Property Type As CoreMessageBox.MessageType

    End Interface
    ''' <summary>
    ''' Interface of the LoginForm the mapped UI class has to fullfill
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface iUINativeFormLogin
        Inherits iUINativeForm


        Property ConfigSet As String
        Property ConfigSetList As List(Of String)
        Property ConfigSetEnabled As Boolean

        Property Username As String
        Property Password As String
        Property Right As String
        Property RightsList As List(Of String)

        Property RightChangeEnabled As String

        Property StatusText As String
        Property UsernameEnabled As Boolean
        Property Message As String
        Property Domain As String
        Property DomainList As List(Of String)

        Property DomainChangeEnables As Boolean


    End Interface

    ''' <summary>
    ''' OTDB UI Module is a static Module for Administration of the UIType <---> Name Mapping
    ''' 
    ''' For each UI Type you have to register the concrete Mapping to use for each Type
    ''' </summary>
    ''' <remarks></remarks>
    Public Module UserInterface

        '** Const of all the Forms we need a mapping for
        Public Const LoginFormName As String = "UILogin"
        Public Const MessageboxFormName As String = "UIMessageBox"

        Private _UIMapping As New Dictionary(Of String, Type)

        ReadOnly Property UITypeFor(otdbUIClassName As String) As Type
            Get
                If _UIMapping.ContainsKey(otdbUIClassName) Then
                    Return _UIMapping.Item(otdbUIClassName)
                Else
                    Return Nothing
                End If
            End Get
        End Property
        Public Function CreateUINew(ByVal otdbUIClassName As String) As Object
            If HasNativeUI(otdbUIClassName) Then
                Dim aType As System.Type = UITypeFor(otdbUIClassName)
                Return aType.GetConstructor(New System.Type() {}).Invoke(New Object() {})
            Else
                Call CoreMessageHandler(message:="UI Class is not registered", arg1:=otdbUIClassName, _
                                       messagetype:=otCoreMessageType.InternalError, noOtdbAvailable:=True, subname:="OTDBUI.createUINew")
                Return Nothing
            End If
        End Function
        Public Function HasNativeUI(ByVal otdbUIClassName As String) As Boolean
            If _UIMapping.ContainsKey(otdbUIClassName) Then
                Return True
            Else
                Return False
            End If

        End Function
        Public Function RegisterNativeUI(ByVal otdbUIClassName As String, nativeType As Type) As Boolean
            If _UIMapping.ContainsKey(otdbUIClassName) Then
                _UIMapping.Remove(otdbUIClassName)
            End If
            _UIMapping.Add(key:=otdbUIClassName, value:=nativeType)

            Return True
        End Function
        Public Function UnRegisterNativeUI(ByVal otdbUIClassName As String, nativeType As Type) As Boolean
            If _UIMapping.ContainsKey(otdbUIClassName) Then
                If _UIMapping.Item(key:=otdbUIClassName) = nativeType Then
                    _UIMapping.Remove(key:=otdbUIClassName)
                    Return True
                End If
            End If


            Return False
        End Function
    End Module

    ''' <summary>
    ''' Abstract Base Class for the OTDB UI Forms
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CoreAbstractForm

        Protected _form As iUINativeForm

        Protected Property form As iUINativeForm
            Set(value As iUINativeForm)
                _form = value
            End Set
            Get
                Return _form
            End Get
        End Property
        Public Sub Show()
            _form.ShowOTDBForm()
        End Sub
        Public Sub Close()
            _form.CloseOTDBForm()
        End Sub

    End Class

    '***********************************************************************
    '***** CLASS clsCoreUIMessagebox is a wrapper class for a MessageBox
    '*****
    '*****
    ''' <summary>
    ''' class is a wrapper abstract class for the Messagebox to OTDB
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Public Class CoreMessageBox
        Inherits CoreAbstractForm
        Implements iOTDBUIAbstractForm

        ''' <summary>
        ''' Button Types
        ''' </summary>
        ''' <remarks></remarks>
        Enum ButtonType
            OK
            YesNo
            YesNoCancel
            OKCancel
        End Enum
        ''' <summary>
        ''' Result
        ''' </summary>
        ''' <remarks></remarks>
        Enum ResultType
            Ok
            Yes
            No
            Cancel
            None
        End Enum
        ''' <summary>
        ''' Type of Messagebox
        ''' </summary>
        ''' <remarks></remarks>
        Enum MessageType
            Warning
            Info
            Question
            [Error]
        End Enum
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shadows _form As iUINativeFormMessageBox

        Public Sub New()
            MyBase.New()
            _form = UserInterface.CreateUINew(UserInterface.MessageboxFormName)
            _form.OtdbShadow = Me
            MyBase.form = _form
            buttons = ButtonType.OK
            type = MessageType.Info
            Message = "here should be a message for you"
        End Sub

        ''' <summary>
        ''' set the Buttons
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [buttons] As ButtonType
            Set(value As ButtonType)
                _form.Buttons = value
            End Set
            Get
                Return _form.Buttons
            End Get
        End Property
        ''' <summary>
        ''' MessageType
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [type] As MessageType
            Set(value As MessageType)
                _form.Type = value
            End Set
            Get
                Return _form.Type
            End Get
        End Property

        ''' <summary>
        ''' result
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [result] As ResultType
            Set(value As ResultType)
                _form.Result = value
            End Set
            Get
                Return _form.Result
            End Get
        End Property
        ''' <summary>
        ''' Message of the Message Box
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Message As String
            Set(value As String)
                _form.Message = value
            End Set
            Get
                Return _form.Message
            End Get
        End Property
        ''' <summary>
        ''' Title of the Messagebox
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Title As String
            Set(value As String)
                _form.Title = value
            End Set
            Get
                Return _form.Title
            End Get
        End Property



    End Class
    '***********************************************************************
    '***** CLASS clsCoreUILogin is a wrapper class for Loggin to OTDB
    '*****
    '*****
    ''' <summary>
    ''' class is a wrapper abstract class for the Login to OTDB
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    Public Class CoreLoginForm
        Inherits CoreAbstractForm
        Implements iOTDBUIAbstractForm

        Protected Shadows _form As iUINativeFormLogin

        Private _username As String = ""
        Private _password As String = ""
        Private _statustext As String = ""
        Private _message As String = ""
        Private _domain As String = ""
        Private _enableUsername As Boolean = True
        Private _enableDomain As Boolean = False
        Private _enableConfigSet As Boolean = False
        Private _enableAccess As Boolean = False
        Private _configset As String = ""

        Private _possibleRights As New List(Of String)
        Private _possibleDomains As New List(Of String)
        Private _possibleConfigSets As New List(Of String)

        Private _ok As Boolean = False
        Private _accessright As otAccessRight

        ''' <summary>
        ''' Constructor with initial database driver
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            _form = UserInterface.CreateUINew(UserInterface.LoginFormName)
            _form.OtdbShadow = Me
            MyBase.form = _form
            Call Initialize()
        End Sub

        ''' <summary>
        ''' Gets or sets the possible config sets.
        ''' </summary>
        ''' <value>The possible config sets.</value>
        Public Property PossibleConfigSets() As List(Of String)
            Get
                Return Me._possibleConfigSets
            End Get
            Set
                Me._possibleConfigSets = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the possible domains.
        ''' </summary>
        ''' <value>The possible domains.</value>
        Public Property PossibleDomains() As List(Of String)
            Get
                Return Me._possibleDomains
            End Get
            Set
                Me._possibleDomains = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the possible rights.
        ''' </summary>
        ''' <value>The possible rights.</value>
        Public Property PossibleRights() As List(Of String)
            Get
                Return Me._possibleRights
            End Get
            Set
                Me._possibleRights = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the enable domain.
        ''' </summary>
        ''' <value>The enable domain.</value>
        Public Property EnableDomain() As Boolean
            Get
                Return Me._enableDomain
            End Get
            Set(value As Boolean)
                Me._enableDomain = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the enable config set.
        ''' </summary>
        ''' <value>The enable config set.</value>
        Public Property EnableChangeConfigSet() As Boolean
            Get
                Return Me._enableConfigSet
            End Get
            Set(value As Boolean)
                Me._enableConfigSet = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the enable acces.
        ''' </summary>
        ''' <value>The enable acces.</value>
        Public Property enableAccess() As Boolean
            Get
                Return Me._enableAccess
            End Get
            Set(value As Boolean)
                Me._enableAccess = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the configset.
        ''' </summary>
        ''' <value>The configset.</value>
        Public Property Configset() As String
            Get
                Return Me._configset
            End Get
            Set(value As String)
                Me._configset = value.Clone
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the domain.
        ''' </summary>
        ''' <value>The domain.</value>
        Public Property Domain() As String
            Get
                Return Me._domain
            End Get
            Set
                Me._domain = Value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the messagetext.
        ''' </summary>
        ''' <value>The messagetext.</value>
        Public Property Messagetext() As String
            Get
                Return _message
            End Get
            Set(value As String)
                _message = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the enable username.
        ''' </summary>
        ''' <value>The enable username.</value>
        Public Property EnableUsername() As Boolean
            Get
                Return _enableUsername
            End Get
            Set(value As Boolean)
                _enableUsername = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the statustext.
        ''' </summary>
        ''' <value>The statustext.</value>
        Public Property Statustext() As String
            Get
                Return Me._statustext
            End Get
            Set(value As String)
                Me._statustext = value
            End Set
        End Property

        Public Sub Initialize(Optional username As String = "", Optional password As String = "")
            _username = username
            _form.Username = username
            _form.Password = password
            _form.UsernameEnabled = True
            _form.Message = ""
            _form.Domain = ""
        End Sub

        Public Property Username As String
            Get
                Username = _username
            End Get
            Set(value As String)
                _username = value
                _form.Username = _username
            End Set
        End Property


        Public Property Password As String
            Get
                Password = _password
            End Get
            Set(value As String)
                _password = value
                _form.Password = value
            End Set
        End Property

        Public Property Ok As Boolean

            Get
                Ok = _ok
            End Get
            Set(value As Boolean)
                _ok = value
            End Set
        End Property


        Public Property Accessright As otAccessRight
            Get
                Accessright = _accessright
            End Get
            Set(value As otAccessRight)
                _accessright = value
                Select Case _accessright
                    Case otAccessRight.[ReadOnly]
                        _form.Right = "ReadOnly"
                    Case otAccessRight.ReadUpdateData
                        _form.Right = "ReadUpdate"
                    Case otAccessRight.AlterSchema
                        _form.Right = "AlterSchema"
                End Select
            End Set
        End Property


        Public Function Verify() As Boolean
            Try
                '** change the config set
                If ot.CurrentConfigSetName <> Me.Configset And Me.EnableChangeConfigSet Then
                    ot.CurrentConfigSetName = Me.Configset
                End If
                Dim aDBDriver As iormDatabaseDriver = ot.CurrentDBDriver

                If aDBDriver Is Nothing Then
                    aDBDriver = CurrentSession.CreateOrGetDatabaseDriver()
                End If

                If aDBDriver Is Nothing Then
                    CoreMessageHandler(showmsgbox:=True, message:="No connection to OnTrack Database is available for verifying the user access - contact your administrator", messagetype:=otCoreMessageType.InternalError, subname:="CoreLoginForm.Verify")
                    Return False
                End If
                '** verify
                Verify = aDBDriver.validateUser(username:=Username, password:=Password, accessRequest:=Me.Accessright, domainid:=Domain)

            Catch ex As Exception
                Me.Statustext = "OnTrack Database not available"
                Call CoreMessageHandler(exception:=ex, subname:="clsUILogin.verify", break:=False)
                Verify = False
            End Try

            If Verify Then
                Me.Statustext = "Welcome to the OnTrack Database !"
                Me._form.RefreshOTDBForm()
                Verify = True
                Me.Ok = Verify

                Exit Function
            Else
                Me.Statustext = "Username is not existing in OnTrack Database ?!"
                Verify = False
                Me.Ok = False
                Exit Function
            End If
        End Function

    End Class
End Namespace