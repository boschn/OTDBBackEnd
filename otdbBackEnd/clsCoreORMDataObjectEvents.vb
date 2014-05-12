
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

        Protected Event OnRelationRetrieveNeeded(sender As Object, e As ormDataObjectRelationEventArgs)
        Protected Event OnRelationCreateNeeded(sender As Object, e As ormDataObjectRelationEventArgs)
        Protected Event OnRelationUpdateNeeded(sender As Object, e As ormDataObjectRelationEventArgs)
        Protected Event OnRelationDeleteNeeded(sender As Object, e As ormDataObjectRelationEventArgs)

        '** Events for the Switch from Runtime Mode on to Off
        Public Event OnSwitchRuntimeOff(sender As Object, e As ormDataObjectEventArgs)
        Public Event OnSwitchRuntimeOn(sender As Object, e As ormDataObjectEventArgs)

        ''' <summary>
        ''' cascade the OnRelationLoadNeeded from RelationManager
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Private Sub ormDataObject_RaiseOnRelationLoadNeeded(sender As Object, e As DataObjectRelationMgr.EventArgs) Handles _relationMgr.OnRelatedObjectsRetrieveRequest
            Dim args As New ormDataObjectRelationEventArgs(e)
            RaiseEvent OnRelationRetrieveNeeded(sender, args)
        End Sub

        ''' <summary>
        ''' cascade the OnRelationLoadNeeded from RelationManager
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Private Sub ormDataObject_RaiseOnRelationCreateNeeded(sender As Object, e As DataObjectRelationMgr.EventArgs) Handles _relationMgr.OnRelatedObjectsCreateRequest
            Dim args As New ormDataObjectRelationEventArgs(e)
            RaiseEvent OnRelationCreateNeeded(sender, args)
        End Sub

        '' <summary>
        ''' cascade the OnRelationLoadNeeded from RelationManager
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Private Sub ormDataObject_RaiseOnRelationUpdateNeeded(sender As Object, e As DataObjectRelationMgr.EventArgs)
            Dim args As New ormDataObjectRelationEventArgs(e)
            RaiseEvent OnRelationUpdateNeeded(sender, args)
        End Sub

        '' <summary>
        ''' cascade the OnRelationLoadNeeded from RelationManager
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks></remarks>

        Private Sub ormDataObject_RaiseOnRelationDeleteNeeded(sender As Object, e As DataObjectRelationMgr.EventArgs)
            Dim args As New ormDataObjectRelationEventArgs(e)
            RaiseEvent OnRelationDeleteNeeded(sender, args)
        End Sub
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
                    If anEntry.EntryType = otObjectEntryType.Column And Not e.Record.HasIndex(anEntry.Tablename & "." & anEntry.ColumnName) Then
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

    ''' <summary>
    ''' Event Arguments for Data Object Events
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormDataObjectEventArgs
        Inherits EventArgs

        Private _Object As ormDataObject
        Private _Record As ormRecord
        Private _DescribedByAttributes As Boolean = False
        Private _UseCache As Boolean = False
        Private _pkarray As Object()
        Private _relationIDs As List(Of String)
        Private _Abort As Boolean = False
        Private _result As Boolean = True
        Private _domainID As String = ConstGlobalDomain
        Private _hasDomainBehavior As Boolean = False
        Private _infusemode As otInfuseMode?
        Private _timestamp As DateTime? = DateTime.Now
        Private _runtimeonly As Boolean = False

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New([object] As ormDataObject, _
                       Optional record As ormRecord = Nothing, _
                       Optional describedByAttributes As Boolean = False, _
                        Optional relationID As List(Of String) = Nothing, _
                        Optional domainID As String = "",
                        Optional domainBehavior As Nullable(Of Boolean) = Nothing, _
                          Optional usecache As Nullable(Of Boolean) = Nothing, _
                        Optional pkarray As Object() = Nothing, _
                        Optional runtimeOnly As Boolean = False, _
                        Optional infuseMode As otInfuseMode? = Nothing, _
                        Optional timestamp? As DateTime = Nothing)
            _Object = [object]
            _Record = record
            _relationIDs = relationID
            _DescribedByAttributes = describedByAttributes
            If _domainID <> "" Then _domainID = domainID
            If domainBehavior.HasValue Then _hasDomainBehavior = domainBehavior
            If usecache.HasValue Then _UseCache = usecache
            If infuseMode.HasValue Then _infusemode = infuseMode
            If timestamp.HasValue Then _timestamp = timestamp
            _pkarray = pkarray
            _result = True
            _runtimeonly = runtimeOnly
            _Abort = False
        End Sub

        ''' <summary>
        ''' Gets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public ReadOnly Property Timestamp() As DateTime?
            Get
                Return Me._timestamp
            End Get
        End Property

        ''' <summary>
        ''' Gets the infusemode.
        ''' </summary>
        ''' <value>The infusemode.</value>
        Public ReadOnly Property Infusemode() As otInfuseMode?
            Get
                Return Me._infusemode
            End Get
        End Property

        ''' <summary>
        ''' Gets the has domain behavior.
        ''' </summary>
        ''' <value>The has domain behavior.</value>
        Public ReadOnly Property HasDomainBehavior() As Boolean
            Get
                Return Me._hasDomainBehavior
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Property DomainID() As String
            Get
                Return Me._domainID
            End Get
            Set(value As String)
                Me._domainID = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public Property RelationIDs() As List(Of String)
            Get
                Return Me._relationIDs
            End Get
            Set(value As List(Of String))
                Me._relationIDs = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the result.
        ''' </summary>
        ''' <value>The result.</value>
        Public Property Result() As Boolean
            Get
                Return Me._result
            End Get
            Set(value As Boolean)
                Me._result = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the pkarray.
        ''' </summary>
        ''' <value>The pkarray.</value>
        Public Property Pkarray() As Object()
            Get
                Return Me._pkarray
            End Get
            Set(value As Object())
                Me._pkarray = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the use cache.
        ''' </summary>
        ''' <value>The use cache.</value>
        Public Property UseCache() As Boolean
            Get
                Return Me._UseCache
            End Get
            Set(value As Boolean)
                Me._UseCache = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the abort.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property AbortOperation() As Boolean
            Get
                Return Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets if to proceed.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property Proceed() As Boolean
            Get
                Return Not Me._Abort
            End Get
            Set(value As Boolean)
                Me._Abort = Not value
                Me._result = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the described by attributes.
        ''' </summary>
        ''' <value>The described by attributes.</value>
        Public ReadOnly Property DescribedByAttributes() As Boolean
            Get
                Return Me._DescribedByAttributes
            End Get
        End Property

        ''' <summary>
        ''' Gets the record.
        ''' </summary>
        ''' <value>The record.</value>
        Public ReadOnly Property Record() As ormRecord
            Get
                Return Me._Record
            End Get
        End Property

        ''' <summary>
        ''' Gets the object.
        ''' </summary>
        ''' <value>The object.</value>
        Public Property DataObject() As ormDataObject
            Get
                Return Me._Object
            End Get
            Set(value As ormDataObject)
                _Object = value
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Event Arguments for Data Object Events
    ''' </summary>
    ''' <remarks></remarks>

    Public Class ormDataObjectRelationEventArgs
        Inherits EventArgs


        Private _timestamp As DateTime = DateTime.Now
        Private _relationEventArgs As DataObjectRelationMgr.EventArgs

        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByRef relationMgrEventArgs As DataObjectRelationMgr.EventArgs, _
                        Optional timestamp? As DateTime = Nothing)
            _relationEventArgs = relationMgrEventArgs
            If timestamp.HasValue Then _timestamp = timestamp
        End Sub

        ''' <summary>
        ''' Gets or sets the relation attribute.
        ''' </summary>
        ''' <value>The relation attribute.</value>
        Public ReadOnly Property RelationAttribute() As ormRelationAttribute
            Get
                Return _relationEventArgs.RelationAttribute
            End Get
        End Property

        ''' <summary>
        ''' Gets the timestamp.
        ''' </summary>
        ''' <value>The timestamp.</value>
        Public ReadOnly Property Timestamp() As DateTime
            Get
                Return Me._timestamp
            End Get
        End Property

        ''' <summary>
        ''' Gets the infusemode.
        ''' </summary>
        ''' <value>The infusemode.</value>
        Public ReadOnly Property Infusemode() As otInfuseMode?
            Get
                Return _relationEventArgs.InfuseMode
            End Get

        End Property
        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public ReadOnly Property RelationObjects() As List(Of iormPersistable)
            Get
                Return _relationEventArgs.Objects
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the relation ID.
        ''' </summary>
        ''' <value>The relation ID.</value>
        Public ReadOnly Property RelationID() As String
            Get
                Return _relationEventArgs.RelationAttribute.Name
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets if to proceed.
        ''' </summary>
        ''' <value>The abort.</value>
        Public Property Finished() As Boolean
            Get
                Return _relationEventArgs.Finished
            End Get
            Set(value As Boolean)
                _relationEventArgs.Finished = value
            End Set
        End Property


        ''' <summary>
        ''' Gets the object.
        ''' </summary>
        ''' <value>The object.</value>
        Public ReadOnly Property DataObject() As ormDataObject
            Get
                Return _relationEventArgs.Dataobject
            End Get
        End Property

    End Class
End Namespace

