

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** classes for error and exception handling
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2013-08-08
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Namespace OnTrack.Database

    ''' <summary>
    ''' ORMException is an Exception for the ORM LAyer
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormException
        Inherits Exception

        Protected _InnerException As Exception
        Protected _message As String
        Protected _subname As String
        Protected _path As String ' Database path
        Public Sub New(Optional message As String = Nothing, Optional exception As Exception = Nothing, Optional subname As String = Nothing, Optional path As String = Nothing)
            If Not String.IsNullOrWhiteSpace(message) Then _message = message
            If Not String.IsNullOrWhiteSpace(subname) Then _subname = subname
            If exception IsNot Nothing Then _InnerException = exception
            If Not String.IsNullOrWhiteSpace(path) Then _path = path
        End Sub

        ''' <summary>
        ''' Gets the path.
        ''' </summary>
        ''' <value>The path.</value>
        Public ReadOnly Property Path() As String
            Get
                Return Me._path
            End Get
        End Property

        ''' <summary>
        ''' Gets the subname.
        ''' </summary>
        ''' <value>The subname.</value>
        Public ReadOnly Property Subname() As String
            Get
                Return Me._subname
            End Get
        End Property

        ''' <summary>
        ''' Gets the message.
        ''' </summary>
        ''' <value>The message.</value>
        Public ReadOnly Property Message() As String
            Get
                Return Me._message
            End Get
        End Property

        ''' <summary>
        ''' Gets the inner exception.
        ''' </summary>
        ''' <value>The inner exception.</value>
        Public ReadOnly Property InnerException() As Exception
            Get
                Return Me._InnerException
            End Get
        End Property

    End Class

    ''' <summary>
    ''' No Connection Excpetion
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ormNoConnectionException
        Inherits ormException
        Public Sub New(Optional message As String = Nothing, Optional exception As Exception = Nothing, Optional subname As String = Nothing, Optional path As String = Nothing)
            MyBase.New(message:=message, exception:=exception, subname:=subname, path:=path)
        End Sub

    End Class

    ''' <summary>
    ''' Event arguments for Ontrack error Events
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormErrorEventArgs
        Inherits EventArgs

        Private _error As SessionMessage

        Public Sub New(newError As SessionMessage)
            _error = newError
        End Sub
        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        ''' <value>The error.</value>
        Public ReadOnly Property [Error]() As SessionMessage
            Get
                Return Me._error
            End Get
        End Property

    End Class
End Namespace
