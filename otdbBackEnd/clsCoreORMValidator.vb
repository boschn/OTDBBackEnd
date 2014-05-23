
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
    ''' ObjectEntry Validation Property Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ObjectValidationProperty
        Inherits AbstractPropertyFunction(Of otObjectValidationProperty)
        Public Const Unique = "UNIQUE"
        Public Const NotEmpty = "NOTEMPTY"
        Public Const UseLookup = "USELOOKUP"
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
        Public Function Apply(ByVal [in] As String()) As Boolean
            Dim result As Boolean = True
            If [in] Is Nothing Then Return True
            For i = 0 To [in].Count - 1
                result = result And Me.Apply([in]:=[in](i))
            Next
            Return result
        End Function
        ''' <summary>
        ''' Apply the Property function to a value
        ''' </summary>
        ''' <param name="in"></param>
        ''' <param name="out"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Apply(ByVal [in] As Object) As Boolean
            Select Case _property
                Case otObjectValidationProperty.Unique
                    Return True
                Case Else
                    CoreMessageHandler(message:="Property function is not implemented", arg1:=_property.ToString, messagetype:=otCoreMessageType.InternalError, _
                                       subname:="ObjectValidationProperty.Apply")
                    Return False
            End Select
        End Function
        ''' <summary>
        ''' returns the enumeration value
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToEnum() As otObjectValidationProperty
            Return AbstractPropertyFunction(Of otObjectValidationProperty).ToEnum(_property)
        End Function
    End Class
    ''' <summary>
    ''' Enumeration of the validation properties
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otObjectValidationProperty
        <Description(ObjectValidationProperty.Unique)> Unique = 1
        <Description(ObjectValidationProperty.NotEmpty)> NotEmpty
        <Description(ObjectValidationProperty.UseLookup)> UseLookup
    End Enum


    ''' <summary>
    ''' type of validation results
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum otValidationResultType
        FailedNoSave = 1
        FailedButSave
        Succeeded
    End Enum

    ''' <summary>
    ''' Validation parts of the ormDataObject Class
    ''' </summary>
    ''' <remarks></remarks>

    Partial Public MustInherit Class ormDataObject

        ''' <summary>
        ''' Raise the Validating Event for this object
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RaiseOnEntryValidatingEvent(entryname As String, msglog As ObjectMessageLog) As otValidationResultType Implements iormValidatable.RaiseOnEntryValidatingEvent
            Dim args As ormDataObjectEntryValidationEventArgs = New ormDataObjectEntryValidationEventArgs(object:=Me, entryname:=entryname, msglog:=msglog, timestamp:=Date.Now)

            RaiseEvent OnEntryValidating(Me, args)
            Return args.Result
        End Function

        ''' <summary>
        ''' Raise the Validated Event for this object
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RaiseOnEntryValidatedEvent(entryname As String, msglog As ObjectMessageLog) As otValidationResultType Implements iormValidatable.RaiseOnEntryValidatedEvent
            Dim args As ormDataObjectEntryValidationEventArgs = New ormDataObjectEntryValidationEventArgs(object:=Me, entryname:=entryname, msglog:=msglog, timestamp:=Date.Now)

            RaiseEvent OnEntryValidated(Me, args)
            Return args.Result
        End Function
        ''' <summary>
        ''' Raise the Validating Event for this object
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RaiseOnValidatingEvent(msglog As ObjectMessageLog) As otValidationResultType Implements iormValidatable.RaiseOnValidatingEvent
            Dim args As ormDataObjectValidationEventArgs = New ormDataObjectValidationEventArgs(object:=Me, msglog:=msglog, timestamp:=Date.Now)

            RaiseEvent OnValidating(Me, args)
            Return args.Result
        End Function

        ''' <summary>
        ''' Raise the Validated Event for this object
        ''' </summary>
        ''' <param name="msglog"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RaiseOnValidatedEvent(msglog As ObjectMessageLog) As otValidationResultType Implements iormValidatable.RaiseOnValidatedEvent
            Dim args As ormDataObjectValidationEventArgs = New ormDataObjectValidationEventArgs(object:=Me, msglog:=msglog, timestamp:=Date.Now)

            RaiseEvent OnValidated(Me, args)
            Return args.Result
        End Function
        ''' <summary>
        ''' validates the Business Object as total
        ''' </summary>
        ''' <remarks></remarks>
        ''' <returns>True if validated and OK</returns>
        Public Function Validate(Optional msglog As ObjectMessageLog = Nothing) As otValidationResultType Implements iormValidatable.Validate
            If msglog Is Nothing Then msglog = Me.ObjectMessageLog
            Dim args As New ormDataObjectValidationEventArgs(object:=Me, timestamp:=Date.Now)
            Dim result As otValidationResultType
            '''
            ''' STEP 1 Raise the pre event
            ''' 
            RaiseEvent OnValidating(Me, args)
            If args.ValidationResult = otValidationResultType.FailedNoSave Then Return args.ValidationResult

            ''' 
            ''' Validate all the Entries against current value
            ''' 
            For Each anEntryname In Me.ObjectDefinition.Entrynames
                result = Me.Validate(entryname:=anEntryname, value:=GetValue(entryname:=anEntryname), msglog:=msglog)
                If result = otValidationResultType.FailedNoSave Then
                    ''' what now
                    ''' 
                    Return result
                End If
            Next
            ''' 
            ''' STEP 3 Raise the validated Event
            ''' 
            RaiseEvent OnValidated(Me, args)
            If args.ValidationResult = otValidationResultType.FailedNoSave Then Return args.ValidationResult

            Return args.ValidationResult
        End Function

        ''' <summary>
        ''' validates a named object entry of the object
        ''' </summary>
        ''' <param name="enryname"></param>
        ''' <remarks></remarks>
        ''' <returns></returns>
        Public Function Validate(entryname As String, ByVal value As Object, Optional msglog As ObjectMessageLog = Nothing) As otValidationResultType Implements iormValidatable.Validate
            Dim result As otValidationResultType

            ''' how to validate during bootstrapping or session starting
            If CurrentSession.IsBootstrappingInstallationRequested OrElse CurrentSession.IsStartingUp Then
                '' while doing it different
                result = otValidationResultType.Succeeded
            Else
                ''' 3 Step Validation process
                ''' 
                If msglog Is Nothing Then msglog = Me.ObjectMessageLog
                Dim args As New ormDataObjectEntryValidationEventArgs(object:=Me, entryname:=entryname, value:=value, msglog:=msglog, timestamp:=Date.Now)

                '''
                ''' STEP 1 RAISE THE VALIDATING ENTRY EVENT BEFORE WE PROCESS
                '''
                RaiseEvent OnEntryValidating(Me, args)
                If args.ValidationResult = otValidationResultType.FailedNoSave Then Return args.ValidationResult
                If args.Result Then value = args.Value

                '''
                '''  STEP 2 Validate the entry against INTERNAL RULES
                ''' 
                result = ObjectValidator.Validate(Me.ObjectDefinition.GetEntry(entryname), newvalue:=value, msglog:=msglog)
                If result = otValidationResultType.FailedNoSave Then Return result

                ''' STEP 3 VALIDATE VIA ENTRY VALIDATED EVENT (Post Validating)
                ''' 
                RaiseEvent OnEntryValidated(Me, args)
                result = args.ValidationResult

                Return result
            End If
            Return result
        End Function

    End Class


    ''' <summary>
    ''' Class for Object (Entry) Validation
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ObjectValidator

        ''' <summary>
        ''' Event Argument Class
        ''' </summary>
        ''' <remarks></remarks>
        Public Class EventArgs
            Inherits System.EventArgs

        End Class

        Private Shared _validate As otValidationResultType


        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Event OnValidationEntyFailed(sender As Object, e As ObjectValidator.EventArgs)


        ''' <summary>
        ''' validate an individual entry (contextfree)
        ''' </summary>
        ''' <param name="objectentrydefinition"></param>
        ''' <param name="newvalue"></param>
        ''' <param name="oldvalue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Validate(objectentrydefinition As iormObjectEntry, ByVal newvalue As Object, _
                                             Optional ByRef msglog As ObjectMessageLog = Nothing) As otValidationResultType

            If objectentrydefinition Is Nothing Then
                CoreMessageHandler(message:="object entry definition is nothing - validate aborted", messagetype:=otCoreMessageType.InternalError, _
                                   subname:="ObjectValidator.ValidateEntry")
                Return otValidationResultType.FailedNoSave
            End If
            Try

                'If msglog Is Nothing Then msglog = New ObjectMessageLog()

                ''' try to convert
                Dim failedflag As Boolean
                Converter.Object2otObject(newvalue, objectentrydefinition.Datatype, isnullable:=objectentrydefinition.IsNullable, failed:=failedflag)
                If failedflag And msglog IsNot Nothing Then
                    If newvalue IsNot Nothing Then
                        msglog.Add(1101, Nothing, Nothing, Nothing, Nothing,
                                   objectentrydefinition.Objectname, objectentrydefinition.Entryname, objectentrydefinition.Datatype.ToString, newvalue, objectentrydefinition.XID)
                    ElseIf Not objectentrydefinition.IsNullable AndAlso newvalue Is Nothing Then
                        msglog.Add(1102, Nothing, Nothing, Nothing, Nothing,
                             objectentrydefinition.Objectname, objectentrydefinition.Entryname, objectentrydefinition.Datatype.ToString, newvalue, objectentrydefinition.XID)
                    End If

                ElseIf failedflag Then
                    Return otValidationResultType.FailedNoSave
                End If

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

            If objectentrydefinition Is Nothing Then
                CoreMessageHandler(message:="entry of object definition is nothing", _
                                    subname:="EntryProperties.Apply", messagetype:=otCoreMessageType.InternalError)
                Return False
            End If

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
                If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not CurrentSession.IsStartingUp AndAlso _
                    Not objectDefinition.IsBootStrappingObject AndAlso objectDefinition.HasEntry(entryname:=entryname) Then

                    If Not objectDefinition.HasEntry(entryname) Then
                        CoreMessageHandler(message:="entry of object definition could not be found", objectname:=objectid, entryname:=entryname, _
                                            subname:="EntryProperties.Apply", messagetype:=otCoreMessageType.InternalError)
                        Return False
                    Else
                        theProperties = objectDefinition.GetEntry(entryname).Properties
                    End If

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
                        CoreMessageHandler(message:="entry of object class description could not be found", objectname:=objectid, entryname:=entryname, _
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