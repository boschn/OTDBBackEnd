
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA OBJECT VALIDATOR CLASSES
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-31
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic

Imports System.Reflection

Namespace OnTrack.Database

    ''' <summary>
    ''' Class for Object (Entry) Validation
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ObjectValidator
        ''' <summary>
        ''' validate an individual entry (contextfree)
        ''' </summary>
        ''' <param name="objectentrydefinition"></param>
        ''' <param name="newvalue"></param>
        ''' <param name="oldvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ValidateEntry(objectentrydefinition As iormObjectEntry, ByVal newvalue As Object, _
                                             Optional ByRef log As ObjectLog = Nothing) As otValidationResultType
            Try
                ''' default values
                If log Is Nothing Then log = New ObjectLog()

                ''' properties
                Dim theProperties As IList(Of ObjectValidationProperty) = objectentrydefinition.ValidationProperties
                If theProperties Is Nothing OrElse theProperties.Count = 0 Then
                    Return otValidationResultType.Succeeded
                End If

                Return otValidationResultType.Succeeded

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectValidator.ValidateEntry")
                Return otValidationResultType.FailedNoSave
            End Try
        End Function

        ''' <summary>
        ''' validate an individual entry (with context)
        ''' </summary>
        ''' <param name="objectentrydefinition"></param>
        ''' <param name="newvalue"></param>
        ''' <param name="oldvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ValidateEntry(objectentrydefinition As iormObjectEntry, dataobject As iormPersistable, _
                                             Optional ByRef log As ObjectLog = Nothing) As otValidationResultType
            Try
                ''' default values
                If log Is Nothing Then log = New ObjectLog()


                Return otValidationResultType.Succeeded
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectValidator.ValidateEntry")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' validate an individual entry (with context)
        ''' </summary>
        ''' <param name="objectentrydefinition"></param>
        ''' <param name="newvalue"></param>
        ''' <param name="oldvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ValidateObject(objectdefinition As ObjectDefinition, dataobject As iormPersistable, _
                                             Optional ByRef log As ObjectLog = Nothing) As otValidationResultType
            Try
                ''' default values
                If log Is Nothing Then log = New ObjectLog()

                Return otValidationResultType.Succeeded
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="ObjectValidator.ValidateEntry")
                Return False
            End Try
        End Function
    End Class
    ''' <summary>
    ''' Class for Object Entry Properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EntryProperties

        ''' <summary>
        ''' apply the object entry properties
        ''' </summary>
        ''' <param name="objectDefinition"></param>
        ''' <param name="entryname"></param>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Apply(objectentrydefinition As iormObjectEntry, ByVal [in] As Object, ByRef out As Object) As Boolean
            Try
                Dim theProperties As IEnumerable(Of ObjectEntryProperty) = objectentrydefinition.Properties
                If theProperties Is Nothing OrElse theProperties.Count = 0 Then
                    out = [in]
                    Return True
                End If

                ''' apply
                ''' 
                '*** return result
                Return EntryProperties.Apply(properties:=theProperties, [in]:=[in], out:=out)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="EntryProperties.Apply")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' apply the object entry properties
        ''' </summary>
        ''' <param name="objectDefinition"></param>
        ''' <param name="entryname"></param>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Apply(objectDefinition As ObjectDefinition, entryname As String, ByVal [in] As Object, ByRef out As Object) As Boolean
            Try
                Dim theProperties As IEnumerable(Of ObjectEntryProperty)
                Dim objectid As String = objectDefinition.ID

                ''' retrieve the properties
                ''' 
                If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso _
                    Not CurrentSession.IsStartingUp AndAlso objectDefinition.IsBootStrappingObject _
                    AndAlso objectDefinition.HasEntry(entryname:=entryname) Then

                    theProperties = objectDefinition.GetEntry(entryname).Properties
                Else
                    Dim anObjectClassDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(objectid)

                    If anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname) IsNot Nothing Then
                        If anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname).HasValueObjectEntryProperties Then

                            theProperties = anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname).ObjectEntryProperties
                            If theProperties Is Nothing Then
                                out = [in]
                                Return True
                            End If

                        Else
                            out = [in]
                            Return True
                        End If

                    Else
                        CoreMessageHandler(message:="entry of object definition could not be found", objectname:=objectid, entryname:=entryname, _
                                            subname:="EntryProperties.Apply", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                End If

                ''' apply
                ''' 
                '*** return result
                Return EntryProperties.Apply(properties:=theProperties, [in]:=[in], out:=out)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="EntryProperties.Apply")
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Apply the ObjectEntryProperties to a value
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Apply(objectid As String, entryname As String, ByVal [in] As Object, ByRef out As Object) As Boolean
            Try
                Dim theProperties As IEnumerable(Of ObjectEntryProperty)
                Dim anObjectClassDescription As ObjectClassDescription
                ''' retrieve the properties
                ''' 
                If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso _
                    Not CurrentSession.IsStartingUp AndAlso ot.GetBootStrapObjectClassIDs.Contains(objectid) Then

                    Dim anObjectDefinition As ObjectDefinition = CurrentSession.Objects.GetObject(objectid:=objectid)
                    If anObjectDefinition.HasEntry(entryname:=entryname) Then
                        theProperties = anObjectDefinition.GetEntry(entryname).Properties
                    End If

                    
                Else
                    anObjectClassDescription = ot.GetObjectClassDescriptionByID(objectid)

                    If anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname) IsNot Nothing Then
                        If anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname).HasValueObjectEntryProperties Then
                            theProperties = anObjectClassDescription.GetObjectEntryAttribute(entryname:=entryname).ObjectEntryProperties
                            If theProperties Is Nothing Then
                                out = [in]
                                Return True
                            End If

                        Else
                            out = [in]
                            Return True
                        End If

                    Else
                        CoreMessageHandler(message:="entry of object definition could not be found", objectname:=objectid, entryname:=entryname, _
                                            subname:="EntryProperties.Apply", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    End If
                End If

                ''' apply
                ''' 
                '*** return result
                Return EntryProperties.Apply(properties:=theProperties, [in]:=[in], out:=out)

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="EntryProperties.Apply")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' apply the object entry properties to an in value and retrieve a out value
        ''' </summary>
        ''' <param name="properties"></param>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Apply(properties As IEnumerable(Of ObjectEntryProperty), ByVal [in] As Object, ByRef out As Object) As Boolean
            Try
                ''' return
                If properties Is Nothing OrElse properties.Count = 0 Then
                    out = [in]
                    Return True
                End If

                ''' Apply all the Entry Properties
                ''' 
                Dim result As Boolean = True
                Dim outvalue As Object
                Dim inarr() As String 'might be a problem
                Dim outarr() As String
                If IsArray([in]) Then
                    inarr = [in]
                    ReDim outarr(inarr.Count - 1)
                End If

                If properties IsNot Nothing Then
                    For Each aProperty In properties
                        If IsArray([in]) Then
                            result = result And aProperty.Apply([in]:=inarr, out:=outarr)
                            If result Then inarr = outarr ' change the in - it is no reference by
                        Else
                            result = result And aProperty.Apply([in]:=[in], out:=outvalue)
                            If result Then [in] = outvalue ' change the in to reflect changes
                        End If

                    Next
                Else
                    CoreMessageHandler(message:="ObjectEntryProperty is nothing", subname:="EntryProperties.Apply", messagetype:=otCoreMessageType.InternalError)

                End If

                ' set the final out value

                If result And Not IsArray([in]) Then
                    '** if we have a value
                    If outvalue IsNot Nothing Then
                        out = outvalue
                    Else
                        '** may be since result is true from the beginning 
                        '** no property might be applied
                        out = [in]
                    End If

                Else
                    '** if we have a value
                    If outvalue IsNot Nothing Then
                        out = outarr
                    Else
                        '** may be since result is true from the beginning 
                        '** no property might be applied
                        out = [in]
                    End If

                End If

                '*** return result
                Return result
            Catch ex As Exception
                CoreMessageHandler(exception:=ex, subname:="EntryProperties.Apply")
                Return False
            End Try

        End Function
    End Class

End Namespace