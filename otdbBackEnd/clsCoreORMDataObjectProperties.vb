﻿
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

    Partial Public MustInherit Class ormDataObject


        ''' <summary>
        ''' Gets the GUID for the Object.
        ''' </summary>
        ''' <value>T</value>
        Public ReadOnly Property Guid() As Guid Implements iormPersistable.GUID
            Get
                Return Me._guid
            End Get
        End Property
        ''' <summary>
        ''' Sets the flag for ignoring the domainentry (delete on domain level)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property IsDomainIgnored As Boolean
            Get
                Return _DomainIsIgnored
            End Get
            Set(value As Boolean)
                SetValue(entryname:=ConstFNIsDomainIgnored, value:=value)
            End Set
        End Property
        ''' <summary>
        ''' Gets the table store.
        ''' </summary>
        ''' <value>The table store.</value>
        Public ReadOnly Property PrimaryTableStore() As iormDataStore Implements iormPersistable.PrimaryTableStore
            Get
                If _record IsNot Nothing AndAlso _record.Alive AndAlso _record.TableStores IsNot Nothing AndAlso _record.TableStores.Count > 0 Then
                    Return _record.GetTablestore(Me.PrimaryTableID)
                    ''' assume about the tablestore to choose
                ElseIf Not Me.RunTimeOnly AndAlso Me.PrimaryTableID IsNot Nothing Then
                    If _defaultdbdriver IsNot Nothing Then Return _defaultdbdriver.GetTableStore(tableID:=Me.PrimaryTableID)
                    Return ot.GetTableStore(tableid:=Me.PrimaryTableID)
                Else
                    Return Nothing
                End If
            End Get
        End Property
        ''' <summary>
        ''' True if a memory data object
        ''' </summary>
        ''' <value>The run time only.</value>
        Public ReadOnly Property RunTimeOnly() As Boolean Implements iormPersistable.RuntimeOnly
            Get
                Return Me._RunTimeOnly
            End Get
        End Property

        ''' <summary>
        ''' returns the object definition associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectDefinition As ObjectDefinition Implements iormPersistable.ObjectDefinition
            Get
                If _objectdefinition Is Nothing Then
                    _objectdefinition = CurrentSession.Objects.GetObject(objectid:=Me.ObjectID)
                End If

                Return _objectdefinition

            End Get
        End Property
        ''' <summary>
        ''' returns the default value for an Entry of this Object
        ''' </summary>
        ''' <param name="entryname"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectEntryDefaultValue(entryname As String) As Object Implements iormInfusable.ObjectEntryDefaultValue
            Get
                If Me.ObjectDefinition Is Nothing Then
                    Dim anEntryAttribute As ormObjectEntryAttribute = Me.ObjectClassDescription.GetObjectEntryAttribute(entryname)
                    If anEntryAttribute Is Nothing Then Throw New ormException(message:="entry name '" & entryname & "' in object class description '" & Me.ObjectID & "' not found", subname:="ormDataObject.ObjectEntryDefaultValue")

                    Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=entryname.ToUpper, value:=anEntryAttribute.DefaultValue)
                    RaiseEvent OnDefaultValueNeeded(Me, args)
                    If args.Result Then
                        Return args.Value
                    Else
                        Return anEntryAttribute.DefaultValue
                    End If
                Else
                    Dim anEntry As iormObjectEntry = Me.ObjectDefinition.GetEntry(entryname)
                    If anEntry Is Nothing Then Throw New ormException(message:="entry name '" & entryname & "' in object '" & Me.ObjectID & "' not found", subname:="ormDataObject.ObjectEntryDefaultValue")

                    Dim args As ormDataObjectEntryEventArgs = New ormDataObjectEntryEventArgs(object:=Me, entryname:=entryname.ToUpper, value:=anEntry.DefaultValue)
                    RaiseEvent OnDefaultValueNeeded(Me, args)
                    If args.Result Then
                        Return args.Value
                    Else
                        Return anEntry.DefaultValue
                    End If
                End If
            End Get
        End Property
        ''' <summary>
        ''' returns the object class description associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectClassDescription As ObjectClassDescription Implements iormPersistable.ObjectClassDescription
            Get
                If _classDescription Is Nothing Then
                    _classDescription = ot.GetObjectClassDescription(Me.GetType)
                End If
                Return _classDescription
            End Get

        End Property
        ''' <summary>
        ''' sets or gets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property ContextIdentifier() As String Implements iormLoggable.ContextIdentifier
            Get
                Return _contextidentifier
            End Get
            Set(value As String)
                _contextidentifier = value
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property TupleIdentifier() As String Implements iormLoggable.TupleIdentifier
            Get
                Return _tupleidentifier
            End Get
            Set(value As String)
                _tupleidentifier = value
            End Set
        End Property
        ''' <summary>
        ''' sets or gets the context identifier
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property EntityIdentifier() As String Implements iormLoggable.EntityIdentifier
            Get
                Return _entityidentifier
            End Get
            Set(value As String)
                _entityidentifier = value
            End Set
        End Property
        ''' <summary>
        ''' returns the object message log for this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ObjectMessageLog As ObjectMessageLog Implements iormLoggable.ObjectMessageLog
            Get
                ''' ObjectMessageLog wil always return something (except for errors while infuse)
                ''' since also there might be messages before the object comes alive
                ''' Infuse will merge the loaded into the current ones
                ''' 
                If _ObjectMessageLog Is Nothing Then
                    If Not Me.RunTimeOnly Then
                        If Me.IsAlive(throwError:=False) AndAlso GetRelationStatus(ConstRMessageLog) = DataObjectRelationMgr.RelationStatus.Unloaded Then InfuseRelation(ConstRMessageLog)
                        If _ObjectMessageLog Is Nothing Then _ObjectMessageLog = New ObjectMessageLog(Me) ' if nothing is loaded because nothing there
                    Else
                        _ObjectMessageLog = New ObjectMessageLog(Me)
                    End If
                End If

                Return _ObjectMessageLog

            End Get
            Set(value As ObjectMessageLog)
                'Throw New InvalidOperationException("setting the Object message log is not allowed")
            End Set
        End Property
        ''' <summary>
        ''' returns the tableschema associated with this data object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TableSchema() As iotDataSchema
            Get
                If Me.PrimaryTableStore IsNot Nothing Then
                    Return Me.PrimaryTableStore.TableSchema
                Else
                    Return Nothing
                End If

            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the domain ID.
        ''' </summary>
        ''' <value>The domain ID.</value>
        Public Overridable Property DomainID() As String Implements iormPersistable.DomainID
            Get
                If Me.ObjectHasDomainBehavior Then
                    Return Me._domainID
                Else
                    Return CurrentSession.CurrentDomainID
                End If
            End Get
            Set(value As String)
                SetValue(ConstFNDomainID, value)
            End Set
        End Property
        ''' <summary>
        '''  gets the DBDriver for the data object to use (real or the default dbdriver)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DatabaseDriver As iormDatabaseDriver Implements iormPersistable.DatabaseDriver
            Get
                If Me.PrimaryTableStore IsNot Nothing Then Return Me.PrimaryTableStore.Connection.DatabaseDriver
                Return _defaultdbdriver
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the PS is initialized.
        ''' </summary>
        ''' <value>The PS is initialized.</value>
        Public ReadOnly Property IsInitialized() As Boolean Implements iormPersistable.IsInitialized
            Get
                Return Me._IsInitialized
            End Get

        End Property
        ''' <summary>
        ''' returns the ObjectID of the Class of this instance
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectID() As String Implements iormPersistable.ObjectID
            Get

                If Me.ObjectClassDescription IsNot Nothing Then
                    Return Me.ObjectClassDescription.ID
                Else
                    CoreMessageHandler("object id for orm data object class could not be found", arg1:=Me.GetType.Name, _
                                        subname:="ormDataObejct.ObjectID", messagetype:=otCoreMessageType.InternalError)
                End If
                Return Nothing
            End Get
        End Property
        ''' <summary>
        ''' returns the Object Tag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectTag() As String
            Get

                Return ConstDelimiter & Me.ObjectID.ToUpper & Converter.Array2otString(Me.ObjectPrimaryKeyValues)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the isDeleted.
        ''' </summary>
        ''' <value>The isDeleted.</value>
        Public ReadOnly Property IsDeleted() As Boolean Implements iormPersistable.IsDeleted
            Get
                Return Me._IsDeleted
            End Get

        End Property

        ''' <summary>
        ''' returns true if object has domain behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDomainBehavior As Boolean Implements iormPersistable.ObjectHasDomainBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDomainBehavior")
                '    Return False
                'End If

                '** to avoid recursion loops for bootstrapping objects during 
                '** startup of session check these out and look into description
                If CurrentSession.IsBootstrappingInstallationRequested _
                    OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescription(Me.ObjectID)
                    If anObjectDecsription IsNot Nothing Then
                        Return anObjectDecsription.ObjectAttribute.AddDomainBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDomainBehavior
                    Return False
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object is cached
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property UseCache As Boolean Implements iormPersistable.useCache
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.UseCache")
                '    Return False
                'End If
                If _useCache.HasValue Then
                    Return _useCache
                Else
                    '** to avoid recursion loops for bootstrapping objects during 
                    '** startup of session check these out and look into description
                    If CurrentSession.IsBootstrappingInstallationRequested _
                        OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                        Dim anObjectDecsription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(Me.ObjectID)
                        If anObjectDecsription IsNot Nothing AndAlso anObjectDecsription.ObjectAttribute.HasValueUseCache Then
                            _useCache = anObjectDecsription.ObjectAttribute.UseCache
                        Else
                            _useCache = False
                        End If
                    Else
                        Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                        If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.UseCache
                        _useCache = False
                    End If

                    Return _useCache
                End If

            End Get

        End Property
        ''' <summary>
        ''' returns true if object has delete per flag behavior
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectHasDeletePerFlagBehavior As Boolean Implements iormPersistable.ObjectHasDeletePerFlagBehavior
            Get
                ' do not initialize
                'If Not _IsInitialized AndAlso Not Initialize() Then
                '    CoreMessageHandler(message:="could not initialize object", subname:="ormDataObject.HasDeletePerFlagBehavior")
                '    Return False
                'End If
                '** avoid loops while starting up with bootstraps or during installation
                If CurrentSession.IsBootstrappingInstallationRequested OrElse ot.GetBootStrapObjectClassIDs.Contains(Me.ObjectID) Then
                    Dim anObjectDescription As ObjectClassDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                    If anObjectDescription IsNot Nothing Then
                        Return anObjectDescription.ObjectAttribute.AddDeleteFieldBehavior
                    Else
                        Return False
                    End If
                Else
                    Dim aObjectDefinition As ObjectDefinition = Me.ObjectDefinition
                    '** per flag
                    If aObjectDefinition IsNot Nothing Then Return aObjectDefinition.HasDeleteFieldBehavior
                End If

            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the changed property
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public Property IsChanged() As Boolean Implements iormPersistable.isChanged
            Get
                Return Me._IsChanged
            End Get
            Protected Friend Set(value As Boolean)
                Me._IsChanged = value
                _changeTimeStamp = DateTime.Now
            End Set
        End Property
        ''' <summary>
        ''' Gets the changed property time stamp
        ''' </summary>
        ''' <value>The PS is changed.</value>
        Public ReadOnly Property ChangeTimeStamp() As DateTime Implements iormPersistable.ChangeTimeStamp
            Get
                Return _changeTimeStamp
            End Get
        End Property
        ''' <summary>
        ''' True if the Object was instanced by Retrieve
        ''' </summary>
        ''' <value>The PS is loaded.</value>
        Public ReadOnly Property IsLoaded() As Boolean Implements iormPersistable.IsLoaded
            Get
                Return _isLoaded
            End Get

        End Property
        ''' <summary>
        ''' returns True if the Object is infused
        ''' </summary>
        ''' <value>The PS is created.</value>
        Public ReadOnly Property IsInfused() As Boolean Implements iormPersistable.IsInfused
            Get
                Return _isInfused
            End Get
        End Property
        ''' <summary>
        '''  returns True if the Object was Instanced by Create
        ''' </summary>
        ''' <value>The PS is created.</value>
        Public ReadOnly Property IsCreated() As Boolean Implements iormPersistable.IsCreated
            Get
                Return _isCreated
            End Get
        End Property
        ''' <summary>
        ''' unload the Dataobject from the datastore
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function Unload() As Boolean
            _isLoaded = False
        End Function
        ''' <summary>
        ''' Gets or sets the OTDB record.
        ''' </summary>
        ''' <value>The OTDB record.</value>
        Public Property Record() As ormRecord Implements iormPersistable.Record
            Get
                Return Me._record
            End Get
            Set(value As ormRecord)
                If _record Is Nothing Then
                    Me._record = value
                Else
                    MergeRecord(value)
                End If
            End Set
        End Property
        ''' <summary>
        ''' returns an array of the primarykey entry names
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectPrimaryKeyEntrynames As String()
            Get
                If (_primarykeynames Is Nothing OrElse _primarykeynames.Length = 0) Then
                    Dim aDescription = ot.GetObjectClassDescriptionByID(id:=Me.ObjectID)
                    _primarykeynames = aDescription.PrimaryKeyEntryNames
                End If

                Return _primarykeynames
            End Get
        End Property
        ''' <summary>
        ''' returns the primaryKeyvalues
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ObjectPrimaryKeyValues As Object() Implements iormPersistable.ObjectPrimaryKeyValues
            Get

                If (_primaryKeyValues Is Nothing OrElse _primaryKeyValues.Length = 0) Then

                    If Me.ObjectPrimaryKeyEntrynames IsNot Nothing AndAlso ObjectPrimaryKeyEntrynames.Length > 0 Then
                        ReDim _primaryKeyValues(Me.ObjectPrimaryKeyEntrynames.Length - 1)

                        For i = 0 To Me.ObjectPrimaryKeyEntrynames.Length - 1
                            If Me.ObjectPrimaryKeyEntrynames(i) IsNot Nothing Then
                                _primaryKeyValues(i) = Me.GetValue(Me.ObjectPrimaryKeyEntrynames(i))
                            End If
                        Next
                    End If
                End If
                Return _primaryKeyValues
            End Get
        End Property
        Public Property LoadedFromHost() As Boolean
            Get
                LoadedFromHost = _IsloadedFromHost
            End Get
            Protected Friend Set(value As Boolean)
                _IsloadedFromHost = value
            End Set
        End Property
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SavedToHost() As Boolean
            Get
                SavedToHost = _IsSavedToHost
            End Get
            Protected Friend Set(value As Boolean)
                _IsSavedToHost = value
            End Set
        End Property
        '** set the serialize with HostApplication
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SerializeWithHostApplication() As Boolean
            Get
                SerializeWithHostApplication = _persistInHostApplication
            End Get
            Protected Friend Set(value As Boolean)
                If value Then
                    If isRegisteredAtHostApplication(Me.PrimaryTableID) Then
                        _persistInHostApplication = True
                    Else
                        _persistInHostApplication = registerHostApplicationFor(Me.PrimaryTableID, AllObjectSerialize:=False)
                    End If
                Else
                    _persistInHostApplication = False
                End If
            End Set
        End Property
        ''' <summary>
        ''' gets the associated tableids of this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property TableIDs As String() Implements iormPersistable.TableIDs
            Get
                ''' to avoid loops get the description here
                If _tableids.Count = 0 Then
                    Dim anObjectDescription As ObjectClassDescription = Me.ObjectClassDescription
                    If anObjectDescription IsNot Nothing Then _tableids = anObjectDescription.ObjectAttribute.Tablenames
                End If

                Return _tableids
            End Get
        End Property

        ''' <summary>
        ''' gets the TableID of the primary Table of this object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property PrimaryTableID() As String Implements iormPersistable.primaryTableID
            Get
                ''' to avoid loops get the description here
                If _primaryTableID = "" Then
                    Dim anObjectDescription As ObjectClassDescription = Me.ObjectClassDescription
                    If anObjectDescription IsNot Nothing Then _primaryTableID = anObjectDescription.ObjectAttribute.Tablenames.First
                End If

                Return _primaryTableID
            End Get
        End Property
        ''' <summary>
        ''' sets or gets the messagelogtag
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MessageLogTag() As String
            Get
                Return _msglogtag
            End Get
            Set(value As String)
                SetValue(ConstFNMSGLOGTAG, value)
            End Set
        End Property
        ''' <summary>
        ''' gets the Creation date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property CreatedOn() As Date?
            Get
                CreatedOn = _createdOn
            End Get
        End Property
        ''' <summary>
        ''' gets the last update date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property UpdatedOn() As Date?
            Get
                UpdatedOn = _updatedOn
            End Get
        End Property
        ''' <summary>
        ''' gets the deletion date in the persistence store
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property DeletedOn() As Date?
            Get
                DeletedOn = _deletedOn
            End Get
            Friend Set(value As Date?)
                SetValue(ConstFNDeletedOn, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_num1() As Double?
            Get
                Return _parameter_num1
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_num2() As Double?
            Get
                Return _parameter_num2
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum2, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter num3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>

        Public Property parameter_num3() As Double?
            Get
                Return _parameter_num3
            End Get
            Set(value As Double?)
                SetValue(ConstFNParamNum3, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter date1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date1() As Date?
            Get
                Return _parameter_date1
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter date2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date2() As Date?
            Get
                Return _parameter_date2
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate2, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the additional spare parameter date3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_date3() As Date?
            Get
                Return _parameter_date3
            End Get
            Set(value As Date?)
                SetValue(ConstFNParamDate3, value)
            End Set
        End Property

        ''' <summary>
        ''' gets or sets the additional spare parameter flag1
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag1() As Boolean?
            Get
                Return _parameter_flag1
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag1, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter flag3
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag3() As Boolean?
            Get
                parameter_flag3 = _parameter_flag3
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag3, value)
            End Set
        End Property
        ''' <summary>
        ''' gets or sets the additional spare parameter flag2
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_flag2() As Boolean?
            Get
                Return _parameter_flag2
            End Get
            Set(value As Boolean?)
                SetValue(ConstFNParamFlag2, value)
            End Set
        End Property

        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt1() As String
            Get
                Return _parameter_txt1
            End Get
            Set(value As String)
                SetValue(ConstFNParamText1, value)
            End Set
        End Property
        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt2() As String
            Get
                Return _parameter_txt2
            End Get
            Set(value As String)
                SetValue(ConstFNParamText2, value)
            End Set
        End Property
        ''' <summary>
        '''  gets or sets the additional spare parameter
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property parameter_txt3() As String
            Get
                Return _parameter_txt3
            End Get
            Set(value As String)
                SetValue(ConstFNParamText3, value)
            End Set
        End Property

    End Class
End Namespace
