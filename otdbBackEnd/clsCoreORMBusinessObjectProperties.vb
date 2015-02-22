
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

    Partial Public MustInherit Class ormBusinessObject

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
        Public ReadOnly Property PrimaryTableStore() As iormRelationalTableStore Implements iormRelationalPersistable.PrimaryTableStore
            Get
                If _record IsNot Nothing AndAlso _record.Alive AndAlso _record.TableStores IsNot Nothing AndAlso _record.TableStores.Count > 0 Then
                    Return _record.GetTablestore(Me.ObjectPrimaryTableID)
                    ''' assume about the tablestore to choose
                ElseIf Not Me.RunTimeOnly AndAlso Me.ObjectPrimaryTableID IsNot Nothing Then
                    If _defaultdbdriver IsNot Nothing Then Return _defaultdbdriver.GetTableStore(tableID:=Me.ObjectPrimaryTableID)
                    Return ot.GetTableStore(tableid:=Me.ObjectPrimaryTableID)
                Else
                    Return Nothing
                End If
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
                        If Me.IsAlive(throwError:=False) AndAlso GetRelationStatus(ConstRMessageLog) = ormRelationManager.RelationStatus.Unloaded Then InfuseRelation(ConstRMessageLog)
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
        Public ReadOnly Property TableSchema() As iormContainerSchema
            Get
                If Me.PrimaryTableStore IsNot Nothing Then
                    Return Me.PrimaryTableStore.ContainerSchema
                Else
                    Return Nothing
                End If

            End Get
        End Property

       
        ''' <summary>
        '''  gets the DBDriver for the data object to use (real or the default dbdriver)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DatabaseDriver As iormRelationalDatabaseDriver Implements iormRelationalPersistable.DatabaseDriver
            Get
                If Me.PrimaryTableStore IsNot Nothing Then Return Me.PrimaryTableStore.Connection.DatabaseDriver
                Return _defaultdbdriver
            End Get
        End Property

        ''' <summary>
        ''' True if the Object was fully instanced by Retrieve or infuse (all tables)
        ''' </summary>
        ''' <value>The PS is loaded.</value>
        Public ReadOnly Property IsLoaded() As Boolean Implements iormRelationalPersistable.IsLoaded
            Get
                If _tableisloaded IsNot Nothing AndAlso _tableisloaded.Length > 0 Then 'do not use alive since this might be recursive
                    For Each aFlag In _tableisloaded
                        If Not aFlag Then Return False
                    Next
                    Return True
                End If

                Return False
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
                    If isRegisteredAtHostApplication(Me.ObjectPrimaryTableID) Then
                        _persistInHostApplication = True
                    Else
                        _persistInHostApplication = registerHostApplicationFor(Me.ObjectPrimaryTableID, AllObjectSerialize:=False)
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
        ReadOnly Property TableIDs As String() Implements iormRelationalPersistable.TableIDs
            Get
                ''' to avoid loops get the description here
                If _tableids.Length = 0 Then
                    Dim anObjectDescription As ObjectClassDescription = Me.ObjectClassDescription
                    If anObjectDescription IsNot Nothing Then _tableids = anObjectDescription.ObjectAttribute.ContainerIDs
                    ReDim Preserve _tableisloaded(_tableids.GetUpperBound(0))
                End If

                Return _tableids
            End Get
        End Property

        ''' <summary>
        ''' gets the TableID of the primary Table for this dataobject object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property ObjectPrimaryTableID() As String Implements iormRelationalPersistable.ObjectPrimaryTableID
            Get
                If String.IsNullOrWhiteSpace(_primaryTableID) Then
                    _primaryTableID = Me.ObjectClassDescription.ObjectAttribute.PrimaryContainerID
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
