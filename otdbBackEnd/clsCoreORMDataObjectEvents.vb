
REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** ORM DATA OBJECT CLASSES
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-01-31
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2013
REM ***********************************************************************************************************************************************''' <summary>

Option Explicit On

Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Reflection

Namespace OnTrack.Database
    ''' <summary>
    ''' Event based parts of the ormDataObject Class
    '''
    ''' </summary>
    ''' <remarks></remarks>
    Partial Public MustInherit Class ormDataObject
        ''' <summary>
        ''' Events
        ''' </summary>
        ''' <remarks></remarks>
        Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

        '** Lifecycle Events
        Public Event OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Implements iormInfusable.OnDefaultValuesNeeded

        Public Shared Event ClassOnRetrieving(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnRetrieved(sender As Object, e As ormDataObjectEventArgs)

        Public Event OnInjected(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnInjected
        Public Event OnInjecting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnInjecting

        Public Shared Event ClassOnInfusing(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnInfused(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnInfusing(sender As Object, e As ormDataObjectEventArgs) Implements iormInfusable.OnInfusing
        Public Event OnInfused(sender As Object, e As ormDataObjectEventArgs) Implements iormInfusable.OnInfused

        Public Shared Event ClassOnColumnMappingInfusing(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnColumnMappingInfused(sender As Object, e As ormDataObjectEventArgs)

        Public Shared Event ClassOnPersisting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnPersisted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnPersisting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnPersisting
        Public Event OnPersisted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnPersisted

        Public Event OnFeeding(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnFeeding
        Public Event OnFed(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnFed
        Public Shared Event ClassOnFeeding(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnFed(sender As Object, e As ormDataObjectEventArgs)

        Public Shared Event ClassOnUnDeleting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnUnDeleted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnUnDeleting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnUnDeleting
        Public Event OnUnDeleted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnUnDeleted

        Public Shared Event ClassOnDeleting(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnDeleted(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnDeleting(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnDeleting
        Public Event OnDeleted(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnDeleted

        Public Shared Event ClassOnCreating(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnCreated(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnCreating(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnCreating
        Public Event OnCreated(sender As Object, e As ormDataObjectEventArgs) Implements iormPersistable.OnCreated

        Public Event OnCloning(sender As Object, e As ormDataObjectEventArgs) Implements iormCloneable.OnCloning
        Public Event OnCloned(sender As Object, e As ormDataObjectEventArgs) Implements iormCloneable.OnCloned
        Public Shared Event ClassOnCloning(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnCloned(sender As Object, e As ormDataObjectEventArgs)

        Public Event OnInitializing(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnInitialized(sender As Object, e As ormDataObjectEventArgs)

        Public Shared Event ClassOnCheckingUniqueness(sender As Object, e As ormDataObjectEventArgs)

        '* Validation Events
        Public Shared Event ClassOnValidating(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnValidated(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnValidating(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidating
        Public Event OnValidated(sender As Object, e As ormDataObjectEventArgs) Implements iormValidatable.OnValidated

        '* relation Events
        Public Shared Event ClassOnCascadingRelation(sender As Object, e As ormDataObjectEventArgs)
        Public Shared Event ClassOnCascadedRelation(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnRelationLoading(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnRelationLoad(sender As Object, e As ormDataObjectEventArgs)

        '** Events for the Switch from Runtime Mode on to Off
        Public Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnSwitchRuntimeOn(sender As Object, e As ormDataObjectEventArgs)


        ''' <summary>
        ''' raises the PropetfyChanged Event
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Sub RaiseObjectEntryChanged(entryname As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(entryname))
        End Sub


        ''' <summary>
        ''' Raise the Instance OnRelationLoading
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub RaiseOnRelationLoading(sender As Object, e As ormDataObjectEventArgs)
            RaiseEvent OnRelationLoading(sender, e)
        End Sub
        ''' <summary>
        ''' Raise the Instance OnRelationLoaded
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Private Sub RaiseOnRelationLoaded(sender As Object, e As ormDataObjectEventArgs)
            RaiseEvent OnRelationLoad(sender, e)
        End Sub

        ''' <summary>
        ''' Event Handler for defaultValues
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Public Sub ormDataObject_OnDefaultValuesNeeded(sender As Object, e As ormDataObjectEventArgs) Handles Me.OnDefaultValuesNeeded
            Dim result As Boolean = True

            '** set the default values of the object
            If Not CurrentSession.IsBootstrappingInstallationRequested AndAlso Not CurrentSession.IsStartingUp Then
                For Each anEntry In e.DataObject.ObjectDefinition.GetEntries
                    ' only the columns
                    If anEntry.IsColumn Then
                        Dim anColumnEntry As ObjectColumnEntry = TryCast(anEntry, ObjectColumnEntry)
                        If anColumnEntry IsNot Nothing And Not e.Record.HasIndex(anColumnEntry.TableName & "." & anColumnEntry.Columnname) Then
                            '' if a default value is neded is decided in the defaultvalue property
                            '' it might be nothing if nullable is true
                            result = result And e.Record.SetValue(anColumnEntry.TableName & "." & anColumnEntry.Columnname, value:=anColumnEntry.Defaultvalue)
                        End If
                    End If
                Next
            Else
                ''' during bootstrapping install or starting up just take the class description values
                ''' 
                For Each anEntry In Me.ObjectClassDescription.ObjectEntryAttributes
                    ' only the columns
                    If anEntry.EntryType = otObjectEntryDefinitiontype.Column And Not e.Record.HasIndex(anEntry.Tablename & "." & anEntry.ColumnName) Then
                        If anEntry.HasValueDefaultValue Then
                            result = result And e.Record.SetValue(anEntry.Tablename & "." & anEntry.ColumnName, value:=Converter.Object2otObject(anEntry.DefaultValue, anEntry.Typeid))
                        ElseIf Not anEntry.HasValueIsNullable OrElse (anEntry.HasValueIsNullable AndAlso Not anEntry.IsNullable) Then
                            result = result And e.Record.SetValue(anEntry.Tablename & "." & anEntry.ColumnName, value:=ot.GetDefaultValue(anEntry.Typeid))
                        End If
                    End If
                Next
            End If


            e.Result = result
            e.Proceed = True
        End Sub
    End Class
End Namespace

