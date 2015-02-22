REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** User Interface - Logical persistable Elements
REM *********** 
REM *********** Version: 2.0
REM *********** Created: 2015-02-13
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2015
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports System.Diagnostics
Imports System.Diagnostics.Debug
Imports OnTrack.Database


Namespace OnTrack.UI

    ''' <summary>
    ''' persistable View Element
    ''' </summary>
    ''' <remarks></remarks>
    <ormObject(version:=1, id:=ViewElement.ConstObjectID, description:="persistable UI view element", _
       modulename:=ConstModuleUIElements, isbootstrap:=True, useCache:=True, adddomainbehavior:=True)> _
    Public Class ViewElement
        Inherits ormBusinessObject

        Public Const ConstObjectID = "UIViewElement"
    End Class

    ''' <summary>
    ''' persistable PanelElelemnt
    ''' </summary>
    ''' <remarks></remarks>
    Public Class PanelElement

    End Class

End Namespace