REM ***********************************************************************************************************************************************
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** relational helper classes
REM ***********
REM *********** Version: X.YY
REM *********** Created: 2014-04-14
REM *********** Last Change:
REM ***********
REM *********** Change Log:
REM ***********
REM *********** (C) by Boris Schneider 2014
REM ***********************************************************************************************************************************************
Option Explicit On
Imports System.Collections.Generic
Imports System.IO
Imports System.Diagnostics.Debug
Imports System.Reflection
Imports OnTrack.Commons


Namespace OnTrack.Database

    ''' <summary>
    '''  Data Object Class is the Persistable data object here we find the relation parts
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Partial Public MustInherit Class ormDataObject

        ''' <summary>
        ''' infuse the relation mapped Members of a dataobject
        ''' </summary>
        ''' <param name="dataobject"></param>
        ''' <param name="classdescriptor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function InfuseRelationMapped(ByRef dataobject As iormPersistable, ByRef classdescriptor As ObjectClassDescription, _
                                                     mode As otInfuseMode, Optional relationid As String = "") As Boolean
            '* Fire Event OnRelationLoading
            Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, Nothing, relationID:=relationid, infuseMode:=mode, runtimeOnly:=dataobject.RuntimeOnly)
            ourEventArgs.Proceed = True
            ourEventArgs.Result = True
            RaiseEvent ClassOnCascadingRelation(dataobject, ourEventArgs)
            dataobject = ourEventArgs.DataObject
            If Not ourEventArgs.Proceed Then Return ourEventArgs.Result

            Try

                '*** Raise Event
                DirectCast(dataobject, ormDataObject).RaiseOnRelationLoading(dataobject, ourEventArgs)
                If Not ourEventArgs.Proceed Then Return ourEventArgs.Result

                '***
                '*** Fill in the relations
                For Each aRelationAttribute In classdescriptor.RelationAttributes
                    '** run through specific event
                    If relationid = "" OrElse relationid.ToLower = aRelationAttribute.Name.ToLower Then
                        Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedRelationFieldInfos(relationName:=aRelationAttribute.Name)

                        For Each aFieldInfo In aFieldList
                            Dim aMappingAttribute = classdescriptor.GetEntryMappingAttributes(aFieldInfo.Name)
                            If (mode And aMappingAttribute.InfuseMode) Then
                                '** get it by primary key and retrieve
                                If aRelationAttribute.HasValueToPrimarykeys Then
                                    Dim anObject = RelationMgr.GetRelatedObjectByRetrieve(attribute:=aRelationAttribute, _
                                                                         dataobject:=dataobject, classdescriptor:=classdescriptor)
                                    If anObject IsNot Nothing Then
                                        '** setfieldvalue by hook or slooow
                                        If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=anObject) Then
                                            Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                    message:="could not object mapped entry", _
                                                    arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)

                                        End If

                                    ElseIf aRelationAttribute.CascadeOnCreate Then
                                        anObject = RelationMgr.GetRelatedObjectByCreate(attribute:=aRelationAttribute, _
                                                                         dataobject:=dataobject, classdescriptor:=classdescriptor)
                                        '** setfieldvalue by hook or slooow
                                        If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=anObject) Then
                                            Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                    message:="could not object mapped entry", _
                                                    arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)

                                        End If
                                    End If

                                    '** get the related objects by query somehow
                                Else
                                    Dim aList As List(Of iormPersistable) = _
                                        RelationMgr.GetRelatedObjects(attribute:=aRelationAttribute, dataobject:=dataobject, classdescriptor:=classdescriptor)

                                    '** if array
                                    If aFieldInfo.FieldType.IsArray Then
                                        '** setfieldvalue by hook or slooow
                                        If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList.ToArray) Then
                                            Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                                   message:="could not object mapped entry", _
                                                                   arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)

                                        End If

                                        '*** Lists
                                    ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IList)) Then
                                        '** setfieldvalue by hook or slooow
                                        If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList) Then
                                            Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                                    message:="could not object mapped entry", _
                                                                    arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)
                                        End If


                                        '*** Dictionary
                                    ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IDictionary)) Then
                                        Dim aDictionary As IDictionary = aFieldInfo.GetValue(dataobject)
                                        Dim typedef As Type() = aFieldInfo.FieldType.GetGenericArguments()

                                        '** create
                                        If aDictionary Is Nothing Then
                                            If aFieldInfo.FieldType.IsGenericType Then
                                                Dim specifictype = aFieldInfo.FieldType.MakeGenericType(typedef)
                                                aDictionary = Activator.CreateInstance(specifictype)
                                            Else
                                                aDictionary = Activator.CreateInstance(aFieldInfo.FieldType)
                                            End If

                                            '** setfieldvalue by hook or slooow
                                            If Not Reflector.SetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aDictionary) Then
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                        message:="could not object mapped entry", _
                                                        arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)

                                            End If
                                        End If

                                        '** assign
                                        For Each anObject In aList
                                            If typedef(0) = GetType(String) Then
                                                Dim aKey As String = ""
                                                For i = 0 To aMappingAttribute.KeyEntries.Count - 1
                                                    If i > 0 Then aKey &= ConstDelimiter
                                                    aKey &= anObject.Record.GetValue(aMappingAttribute.KeyEntries(i)).ToString
                                                Next
                                                If Not aDictionary.Contains(key:=aKey) Then
                                                    aDictionary.Add(key:=aKey, value:=anObject)
                                                Else
                                                    CoreMessageHandler(message:="for relation '" & aRelationAttribute.Name & "' :key in dictionary member '" & aFieldInfo.Name & "' already exists", _
                                                                       messagetype:=otCoreMessageType.InternalWarning, _
                                                                       objectname:=dataobject.ObjectID, tablename:=dataobject.primaryTableID, _
                                                                       arg1:=aKey, subname:="ormDataObject.InfuseRelationMapped")
                                                End If


                                            ElseIf typedef(0).Equals(GetType(Int64)) And IsNumeric(anObject.Record.GetValue(aMappingAttribute.KeyEntries(0))) Then
                                                Dim aKey As Long = CLng(anObject.Record.GetValue(aMappingAttribute.KeyEntries(0)))
                                                If Not aDictionary.Contains(key:=aKey) Then
                                                    aDictionary.Add(key:=aKey, value:=anObject)
                                                Else
                                                    CoreMessageHandler(message:="for relation '" & aRelationAttribute.Name & "' :key in dictionary member '" & aFieldInfo.Name & "' already exists", _
                                                                       messagetype:=otCoreMessageType.InternalWarning, _
                                                                       objectname:=dataobject.ObjectID, tablename:=dataobject.primaryTableID, _
                                                                       arg1:=aKey, subname:="ormDataObject.InfuseRelationMapped")
                                                End If
                                            Else
                                                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", message:="cannot convert key to dicitionary from List of iormpersistables", _
                                                                        objectname:=dataobject.ObjectID, tablename:=dataobject.primaryTableID)
                                            End If
                                        Next


                                        '*** relationCollection
                                    ElseIf Reflector.TypeImplementsGenericInterface(aFieldInfo.FieldType, GetType(iormRelationalCollection(Of ))) Then
                                        Dim aCollection As Object
                                        If Not Reflector.GetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aCollection) Then
                                            Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", _
                                                            message:="iormRelationalCollection must not be nothing", _
                                                            arg1:=aFieldInfo.Name, objectname:=dataobject.ObjectID, entryname:=aMappingAttribute.EntryName, tablename:=dataobject.primaryTableID)
                                        End If

                                        '** assign
                                        For Each anObject In aList
                                            aCollection.Add(anObject)
                                        Next
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next

                '* Fire Event OnRelationLoading
                ourEventArgs = New ormDataObjectEventArgs(dataobject, Nothing, , relationID:=relationid, infuseMode:=mode, runtimeOnly:=dataobject.RuntimeOnly)
                '*** Raise Event
                DirectCast(dataobject, ormDataObject).RaiseOnRelationLoaded(dataobject, ourEventArgs)
                If Not ourEventArgs.Proceed Then Return False

                '* Fire Event OnRelationLoading
                RaiseEvent ClassOnCascadedRelation(dataobject, ourEventArgs)
                If ourEventArgs.Result Then dataobject = ourEventArgs.DataObject
                Return ourEventArgs.Proceed
            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", exception:=ex, objectname:=dataobject.ObjectID, _
                                        tablename:=dataobject.primaryTableID)
                Return False

            End Try

        End Function
        ''' <summary>
        ''' cascade the update of relational data
        ''' </summary>
        ''' <param name="dataobject"></param>
        ''' <param name="classdescriptor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function CascadeRelation(ByRef dataobject As iormPersistable, ByRef classdescriptor As ObjectClassDescription, _
                                                      cascadeUpdate As Boolean, cascadeDelete As Boolean, _
                                                      Optional relationid As String = "", _
                                                      Optional timestamp As DateTime = constNullDate, _
                                                      Optional uniquenesswaschecked As Boolean = True) As Boolean

            If timestamp = constNullDate Then timestamp = DateTime.Now

            '* Fire Event OnRelationLoading
            Dim ourEventArgs As New ormDataObjectEventArgs(dataobject, Nothing, relationID:=relationid, timestamp:=timestamp)
            ourEventArgs.Proceed = True
            ourEventArgs.Result = True
            RaiseEvent ClassOnCascadingRelation(dataobject, ourEventArgs)
            dataobject = ourEventArgs.DataObject
            If Not ourEventArgs.Proceed Then Return ourEventArgs.Result


            Try
                SyncLock dataobject
                    '***
                    '*** Fill in the relations
                    For Each aRelationAttribute In classdescriptor.RelationAttributes

                        '** run through specific relation condition 
                        If (relationid = "" OrElse relationid.ToLower = aRelationAttribute.Name.ToLower) And _
                            ((cascadeUpdate AndAlso cascadeUpdate = aRelationAttribute.CascadeOnUpdate) OrElse _
                             (cascadeDelete AndAlso cascadeDelete = aRelationAttribute.CascadeOnDelete)) Then
                            '* get the list
                            Dim aFieldList As List(Of FieldInfo) = classdescriptor.GetMappedRelationFieldInfos(relationName:=aRelationAttribute.Name)

                            For Each aFieldInfo In aFieldList
                                Dim aMappingAttribute = classdescriptor.GetEntryMappingAttributes(aFieldInfo.Name)

                                '** if direct persistable
                                If aFieldInfo.FieldType.GetInterfaces().Contains(GetType(iormPersistable)) Then

                                    Dim anobject As Object
                                    '** get value 
                                    If Not Reflector.GetFieldValue(aFieldInfo, dataobject, anobject) Then
                                        anobject = aFieldInfo.GetValue(dataobject)
                                    End If

                                    Dim ansubdataobject = TryCast(anobject, iormPersistable)
                                    If ansubdataobject IsNot Nothing Then
                                        If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                            '** persist
                                            ansubdataobject.Persist(timestamp)
                                        ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                            '** persist
                                            ansubdataobject.Delete(timestamp)
                                        End If
                                    Else
                                        CoreMessageHandler(message:="mapped field in data object does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                           messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                        Return False
                                    End If

                                    '** get the related objects by query somehow
                                Else


                                    '** and Dicitionary
                                    If aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IDictionary)) Then
                                        Dim aDictionary As IDictionary
                                        '** get values either by hook or by slow getvalue
                                        If Not Reflector.GetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aDictionary) Then
                                            aDictionary = aFieldInfo.GetValue(dataobject)
                                        End If
                                        For Each anEntry In aDictionary.Values
                                            Dim anSubdataObject As iormPersistable = TryCast(anEntry, iormPersistable)
                                            If anSubdataObject IsNot Nothing Then
                                                ''' CASCADE UPDATE
                                                If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                                    '** persist
                                                    anSubdataObject.Persist(timestamp)

                                                    ''' CASCADE DELETE
                                                ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                                    '** delete
                                                    anSubdataObject.Delete(timestamp)
                                                End If
                                            Else
                                                CoreMessageHandler(message:="mapped inner field in dictionary object of type enumerable does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                           messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                                Return False
                                            End If
                                        Next

                                        '** run through the enumerables and try to cascade
                                    ElseIf aFieldInfo.FieldType.GetInterfaces.Contains(GetType(IEnumerable)) Then
                                        Dim aList As IEnumerable
                                        '** get values either by hook or by slow getvalue
                                        If Not Reflector.GetFieldValue(field:=aFieldInfo, dataobject:=dataobject, value:=aList) Then
                                            aList = aFieldInfo.GetValue(dataobject)
                                        End If
                                        If aList Is Nothing Then
                                            CoreMessageHandler(message:="mapped inner field in container object of type enumerable is not initialized in class", subname:="ormDataObject.CascadeRelation", _
                                                               messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                            Return False
                                        Else
                                            For Each anEntry In aList
                                                Dim anSubdataObject As iormPersistable
                                                If anEntry.GetType.Equals(GetType(KeyValuePair(Of ,))) Then
                                                    Throw New NotImplementedException
                                                Else
                                                    anSubdataObject = TryCast(anEntry, iormPersistable)
                                                End If
                                                If anSubdataObject IsNot Nothing Then
                                                    If cascadeUpdate = aRelationAttribute.CascadeOnUpdate Then
                                                        '** persist
                                                        anSubdataObject.Persist(timestamp)
                                                    ElseIf cascadeDelete = aRelationAttribute.CascadeOnDelete Then
                                                        '** persist
                                                        anSubdataObject.Delete(timestamp)
                                                    End If
                                                Else
                                                    CoreMessageHandler(message:="mapped inner field in container object of type enumerable does not implement the iormpersistable", subname:="ormDataObject.CascadeRelation", _
                                                               messagetype:=otCoreMessageType.InternalError, objectname:=dataobject.ObjectID, arg1:=aFieldInfo.Name)
                                                    Return False
                                                End If
                                            Next
                                        End If



                                    Else
                                        CoreMessageHandler(message:="generic data handling container object neither of enumerable or dictionary", _
                                                            subname:="ormDataObject.CascadeRelation", messagetype:=otCoreMessageType.InternalError)
                                    End If

                                    ''' if we are not loaded with check on uniqueness
                                    ''' and cascade the relation updates
                                    ''' we need to make sure that all older relations are deleted
                                    If cascadeUpdate = aRelationAttribute.CascadeOnUpdate AndAlso Not uniquenesswaschecked Then
                                        RelationMgr.DeleteRelatedObjects(aRelationAttribute, dataobject:=dataobject, classdescriptor:=classdescriptor, _
                                                                        timestamp:=timestamp)
                                    End If
                                End If

                            Next
                        End If
                    Next

                End SyncLock

                '* Fire Event OnRelationLoading
                ourEventArgs = New ormDataObjectEventArgs(dataobject, Nothing, , relationID:=relationid)
                ourEventArgs.Proceed = True
                ourEventArgs.Result = True
                RaiseEvent ClassOnCascadedRelation(dataobject, ourEventArgs)
                dataobject = ourEventArgs.DataObject
                Return ourEventArgs.Result
            Catch ex As Exception
                Call CoreMessageHandler(subname:="ormDataObject.InfuseRelationMapped", exception:=ex, objectname:=dataobject.ObjectID, _
                                        tablename:=dataobject.primaryTableID)
                Return False

            End Try

        End Function
    End Class
    Public Class RelationMgr


        ''' <summary>
        ''' create a  related objects from a relation attribute for a object class described by a classdescriptor
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="classdescriptor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetRelatedObjectByCreate(attribute As ormSchemaRelationAttribute, dataobject As iormPersistable, classdescriptor As ObjectClassDescription) As iormPersistable
            Dim theKeyvalues As New List(Of Object)
            Dim keyentries As String()

            '** get the keys althoug determining if TOEntries are by Primarykey is a bit obsolete
            If attribute.HasValueToPrimarykeys Then
                keyentries = attribute.ToPrimaryKeys
            Else
                CoreMessageHandler(message:="relation attribute has no ToPrimarykeys set - unable to create", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetObjectByCreate", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Try
                Dim aTargetObjectDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(attribute.LinkObject)
                Dim aTargetType As System.Type = aTargetObjectDescriptor.Type
                theKeyvalues = Reflector.GetValues(dataobject:=dataobject, entrynames:=keyentries)
                Dim runtimeOnly As Boolean = CurrentSession.IsBootstrappingInstallationRequested ' only on runtime if we are bootstrapping
                Dim createMethod = ot.GetMethodInfo(aTargetType, ObjectClassDescription.ConstMTCreateDataObject)

                If createMethod IsNot Nothing Then
                    '** if creating then do also with the new data object in the runtime
                    Dim anObject As iormPersistable = createMethod.Invoke(Nothing, {theKeyvalues.ToArray, "", True, runtimeOnly})
                    Return anObject
                Else
                    CoreMessageHandler(message:="the RETRIEVE method was not found on this object class", messagetype:=otCoreMessageType.InternalError, _
                                        objectname:=aTargetType.Name, subname:="Reflector.GetObjectByCreate")
                    Return Nothing
                End If

                '*** return
                Return Nothing

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetRelatedObjectByCreate")
                Return Nothing
            End Try

        End Function
        ''' <summary>
        ''' retrieves a list of related objects from a relation attribute for a object class described by a classdescriptor
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="dataobject"></param>
        ''' <param name="classdescriptor"></param>
        ''' <param name="dbdriver"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetRelatedObjects(attribute As ormSchemaRelationAttribute, _
                                                 dataobject As iormPersistable, _
                                                 classdescriptor As ObjectClassDescription, _
                                                 Optional dbdriver As iormDatabaseDriver = Nothing) As List(Of iormPersistable)
            Dim theKeyvalues As New List(Of Object)
            Dim theObjectList As New List(Of iormPersistable)
            If dbdriver Is Nothing Then dbdriver = dataobject.DatabaseDriver
            If dbdriver Is Nothing Then dbdriver = CurrentDBDriver
            Dim aTargetObjectDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(attribute.LinkObject)
            If aTargetObjectDescriptor Is Nothing Then
                CoreMessageHandler(message:="class description for class of" & attribute.LinkObject.FullName & " could not be retrieved", arg1:=attribute.Name, subname:="Reflector.GetRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList
            End If
            Dim aTargetType As System.Type = aTargetObjectDescriptor.Type

            Dim domainBehavior As Boolean
            Dim deletebehavior As Boolean
            Dim FNDomainID As String = Domain.ConstFNDomainID
            Dim FNDeleted As String = ConstFNIsDeleted
            Dim domainID As String = CurrentSession.CurrentDomainID
            Dim fromTablename As String = classdescriptor.Tables.First
            Dim toTablename = aTargetObjectDescriptor.Tables.First ' First Tablename if multiple


            '** get the keys althoug determining if TOEntries are by Primarykey is a bit obsolete
            If Not attribute.HasValueFromEntries OrElse Not attribute.HasValueToEntries Then
                CoreMessageHandler(message:="relation attribute has nor fromEntries or ToEntries set", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList
            ElseIf attribute.ToEntries.Count > attribute.FromEntries.Count Then
                CoreMessageHandler(message:="relation attribute has nor mot ToEntries than FromEntries set", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList

            End If

            If Not aTargetType.GetInterfaces.Contains(GetType(iormPersistable)) And Not aTargetType.GetInterfaces.Contains(GetType(iormInfusable)) Then
                CoreMessageHandler(message:="target type has neither iormperistable nor iorminfusable interface", _
                                   arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                    subname:="Reflector.GetRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList
            End If
            '***
            Try
                '** return if we are bootstrapping
                If CurrentSession.IsBootstrappingInstallationRequested Then
                    CoreMessageHandler(message:="query for relations not possible during bootstrapping installation", _
                                        arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                         subname:="Reflector.GetRelatedObjects", messagetype:=otCoreMessageType.InternalWarning)
                    Return theObjectList

                    '** avoid loops during startup
                ElseIf CurrentSession.IsStartingUp AndAlso ot.GetBootStrapObjectClassIDs.Contains(aTargetObjectDescriptor.ID) Then
                    Dim anObjectClassdDescription = ot.GetObjectClassDescriptionByID(id:=aTargetObjectDescriptor.ID)
                    domainBehavior = anObjectClassdDescription.ObjectAttribute.AddDomainBehavior
                    deletebehavior = anObjectClassdDescription.ObjectAttribute.AddDeleteFieldBehavior

                    '** normal way
                Else
                    Dim anObjectDefinition As ObjectDefinition = ot.CurrentSession.Objects.GetObject(objectid:=aTargetObjectDescriptor.ID)
                    domainBehavior = anObjectDefinition.HasDomainBehavior
                    deletebehavior = anObjectDefinition.HasDeleteFieldBehavior
                End If
                theKeyvalues = Reflector.GetValues(dataobject:=dataobject, entrynames:=attribute.FromEntries)
                Dim wherekey As String = ""

                '** get a Store
                Dim aStore As iormDataStore = dbdriver.GetTableStore(toTablename)
                Dim aCommand As ormSqlSelectCommand = aStore.CreateSqlSelectCommand(id:="allbyRelation" & attribute.Name, addAllFields:=True)
                If Not aCommand.Prepared Then
                    ' build the key part
                    For i = 0 To attribute.ToEntries.Count - 1
                        If i > 0 Then wherekey &= " AND "
                        '** if where is run against select of datatable the tablename is creating an error
                        wherekey &= "[" & attribute.ToEntries(i) & "] = @" & attribute.ToEntries(i)
                    Next
                    aCommand.Where = wherekey
                    If attribute.HasValueLinkJOin Then
                        aCommand.Where &= " " & attribute.LinkJoin
                    End If
                    '** additional behavior
                    If deletebehavior Then aCommand.Where &= " AND " & FNDeleted & " = @deleted "
                    If domainBehavior Then aCommand.Where &= " AND ([" & FNDomainID & "] = @domainID OR [" & FNDomainID & "] = @globalID)"

                    '** parameters
                    For i = 0 To attribute.ToEntries.Count - 1
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & attribute.ToEntries(i), columnname:=attribute.ToEntries(i), tablename:=toTablename))
                    Next
                    If deletebehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=FNDeleted, tablename:=toTablename))
                    If domainBehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=FNDomainID, tablename:=toTablename))
                    If domainBehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=FNDomainID, tablename:=toTablename))
                    aCommand.Prepare()
                End If
                '** parameters
                For i = 0 To attribute.ToEntries.Count - 1
                    aCommand.SetParameterValue(ID:="@" & attribute.ToEntries(i), value:=theKeyvalues(i))
                Next
                '** set the values
                If aCommand.HasParameter(ID:="@deleted") Then aCommand.SetParameterValue(ID:="@deleted", value:=False)
                If aCommand.HasParameter(ID:="@domainID") Then aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                If aCommand.HasParameter(ID:="@globalID") Then aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)

                ' Infuse
                Dim aRecordCollection As List(Of ormRecord) = aCommand.RunSelect
                If aRecordCollection Is Nothing Then
                    CoreMessageHandler(message:="no records returned due to previous errors", subname:="Reflector.GetRelatedObjects", arg1:=attribute.Name, _
                                        objectname:=aTargetObjectDescriptor.ObjectAttribute.ID, tablename:=toTablename, messagetype:=otCoreMessageType.InternalError)
                    Return theObjectList
                End If
                Dim aDomainRecordCollection As New Dictionary(Of String, ormRecord)
                Dim pknames = aStore.TableSchema.PrimaryKeys
                For Each aRecord As ormRecord In aRecordCollection

                    If domainBehavior And domainID <> ConstGlobalDomain Then
                        '** build pk key
                        Dim pk As String = ""
                        For Each acolumnname In pknames
                            If acolumnname <> FNDomainID Then pk &= aRecord.GetValue(index:=acolumnname).ToString & ConstDelimiter
                        Next
                        If aDomainRecordCollection.ContainsKey(pk) Then
                            Dim anotherRecord = aDomainRecordCollection.Item(pk)
                            If anotherRecord.GetValue(FNDomainID).ToString = ConstGlobalDomain Then
                                aDomainRecordCollection.Remove(pk)
                                aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                            End If
                        Else
                            aDomainRecordCollection.Add(key:=pk, value:=aRecord)
                        End If
                    Else
                        Dim atargetobject = Activator.CreateInstance(aTargetType)
                        If DirectCast(atargetobject, iormInfusable).Infuse(aRecord) Then
                            theObjectList.Add(DirectCast(atargetobject, iormPersistable))
                        End If
                    End If
                Next

                '** sort out the domains
                If domainBehavior And domainID <> ConstGlobalDomain Then
                    For Each aRecord In aDomainRecordCollection.Values
                        Dim atargetobject = Activator.CreateInstance(aTargetType)
                        If ormDataObject.InfuseDataObject(record:=aRecord, dataobject:=TryCast(atargetobject, iormInfusable), _
                                                          mode:=otInfuseMode.OnInject Or otInfuseMode.OnDefault) Then
                            theObjectList.Add(DirectCast(atargetobject, iormPersistable))
                        End If
                    Next
                End If

                'return finally
                Return theObjectList


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetRelatedObjects")
                Return theObjectList
            End Try




        End Function
        ''' <summary>
        ''' deletes related objects from a relation attribute for a object class described by a classdescriptor
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="dataobject"></param>
        ''' <param name="classdescriptor"></param>
        ''' <param name="dbdriver"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DeleteRelatedObjects(attribute As ormSchemaRelationAttribute, _
                                                 dataobject As iormPersistable, _
                                                 classdescriptor As ObjectClassDescription, _
                                                 Optional dbdriver As iormDatabaseDriver = Nothing, _
                                                 Optional timestamp As DateTime? = Nothing) As List(Of iormPersistable)
            Dim theKeyvalues As New List(Of Object)
            Dim theObjectList As New List(Of iormPersistable)
            If dbdriver Is Nothing Then dbdriver = dataobject.DatabaseDriver
            If dbdriver Is Nothing Then dbdriver = CurrentDBDriver
            Dim aTargetObjectDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(attribute.LinkObject)
            Dim aTargetType As System.Type = aTargetObjectDescriptor.Type

            Dim domainBehavior As Boolean
            Dim deletebehavior As Boolean
            Dim FNDomainID As String = Domain.ConstFNDomainID
            Dim FNDeleted As String = ConstFNIsDeleted
            Dim domainID As String = CurrentSession.CurrentDomainID
            Dim fromTablename As String = classdescriptor.Tables.First
            Dim toTablename = aTargetObjectDescriptor.Tables.First ' First Tablename if multiple


            '** get the keys althoug determining if TOEntries are by Primarykey is a bit obsolete
            If Not attribute.HasValueFromEntries OrElse Not attribute.HasValueToEntries Then
                CoreMessageHandler(message:="relation attribute has nor fromEntries or ToEntries set", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.DeleteRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList
            ElseIf attribute.ToEntries.Count > attribute.FromEntries.Count Then
                CoreMessageHandler(message:="relation attribute has nor mot ToEntries than FromEntries set", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.DeleteRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList

            End If

            If Not aTargetType.GetInterfaces.Contains(GetType(iormPersistable)) And Not aTargetType.GetInterfaces.Contains(GetType(iormInfusable)) Then
                CoreMessageHandler(message:="target type has neither iormperistable nor iorminfusable interface", _
                                   arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                    subname:="Reflector.DeleteRelatedObjects", messagetype:=otCoreMessageType.InternalError)
                Return theObjectList
            End If
            '***
            Try
                '** return if we are bootstrapping
                'If CurrentSession.IsBootstrappingInstallationRequested Then
                '    CoreMessageHandler(message:="query for relations not possible during bootstrapping installation", _
                '                        arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                '                         subname:="Reflector.DeleteRelatedObjects", messagetype:=otCoreMessageType.InternalWarning)
                '    Return theObjectList

                '    '** avoid loops during startup
                'ElseIf CurrentSession.IsStartingUp AndAlso ot.GetBootStrapObjectClassIDs.Contains(aTargetObjectDescriptor.ID) Then
                '    Dim anObjectClassdDescription = ot.GetObjectClassDescriptionByID(id:=aTargetObjectDescriptor.ID)
                '    domainBehavior = anObjectClassdDescription.ObjectAttribute.AddDomainBehavior
                '    deletebehavior = anObjectClassdDescription.ObjectAttribute.AddDeleteFieldBehavior

                '    '** normal way
                'Else
                '    Dim anObjectDefinition As ObjectDefinition = ot.CurrentSession.Objects.GetObject(objectid:=aTargetObjectDescriptor.ID)
                '    domainBehavior = anObjectDefinition.HasDomainBehavior
                '    deletebehavior = anObjectDefinition.HasDeleteFieldBehavior
                'End If
                theKeyvalues = Reflector.GetValues(dataobject:=dataobject, entrynames:=attribute.FromEntries)
                Dim wherekey As String = ""

                '** get a Store
                Dim aStore As iormDataStore = dbdriver.GetTableStore(toTablename)
                Dim aCommand As ormSqlCommand = aStore.CreateSqlCommand(id:="DeleteAllbyRelation_" & attribute.Name)
                If Not aCommand.Prepared Then
                    aCommand.DatabaseDriver = dbdriver
                    Dim aSqlText = String.Format("DELETE FROM {0} WHERE ", toTablename)
                    ' build the key part
                    For i = 0 To attribute.ToEntries.Count - 1
                        If i > 0 Then aSqlText &= " AND "
                        '** if where is run against select of datatable the tablename is creating an error
                        aSqlText &= "[" & attribute.ToEntries(i) & "] = @" & attribute.ToEntries(i)
                    Next

                    If attribute.HasValueLinkJOin Then
                        aSqlText &= " " & attribute.LinkJoin
                    End If
                    '** additional behavior
                    If timestamp.HasValue Then aSqlText &= " AND [" & ConstFNUpdatedOn & "] < @" & ConstFNUpdatedOn
                    'If deletebehavior Then aSqlText &= " AND " & FNDeleted & " = @deleted "
                    'If domainBehavior Then aSqlText &= " AND ([" & FNDomainID & "] = @domainID OR [" & FNDomainID & "] = @globalID)"

                    '** parameters
                    For i = 0 To attribute.ToEntries.Count - 1
                        Dim anEntryAttribute As ormObjectEntryAttribute = classdescriptor.GetObjectEntryAttribute(entryname:=attribute.ToEntries(i))
                        aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & attribute.ToEntries(i), datatype:=anEntryAttribute.Typeid, notColumn:=True))
                    Next
                    If timestamp.HasValue Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@" & ConstFNUpdatedOn, datatype:=otDataType.Timestamp, notColumn:=True))
                    'If deletebehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@deleted", ColumnName:=FNDeleted, tablename:=toTablename))
                    'If domainBehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@domainID", ColumnName:=FNDomainID, tablename:=toTablename))
                    'If domainBehavior Then aCommand.AddParameter(New ormSqlCommandParameter(ID:="@globalID", ColumnName:=FNDomainID, tablename:=toTablename))
                    aCommand.CustomerSqlStatement = aSqlText
                    aCommand.Prepare()
                End If
                '** parameters
                For i = 0 To attribute.ToEntries.Count - 1
                    aCommand.SetParameterValue(ID:="@" & attribute.ToEntries(i), value:=theKeyvalues(i))
                Next
                '** set the values
                If timestamp.HasValue Then aCommand.SetParameterValue(ID:="@" & ConstFNUpdatedOn, value:=timestamp)
                'If aCommand.HasParameter(ID:="@deleted") Then aCommand.SetParameterValue(ID:="@deleted", value:=False)
                'If aCommand.HasParameter(ID:="@domainID") Then aCommand.SetParameterValue(ID:="@domainID", value:=domainID)
                'If aCommand.HasParameter(ID:="@globalID") Then aCommand.SetParameterValue(ID:="@globalID", value:=ConstGlobalDomain)

                ' Infuse
                If Not aCommand.Run() Then
                    CoreMessageHandler(message:="command failed to run", subname:="Reflector.DeleteRelatedObjects", messagetype:=otCoreMessageType.InternalError, _
                                       arg1:=aCommand.SqlText)
                End If

                'return finally
                Return theObjectList


            Catch ex As Exception
                CoreMessageHandler(exception:=ex, _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.DeleteRelatedObjects")
                Return theObjectList
            End Try




        End Function
        ''' <summary>
        ''' retrieves a  related objects from a relation attribute for a object class described by a classdescriptor
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="classdescriptor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetRelatedObjectByRetrieve(attribute As ormSchemaRelationAttribute, dataobject As iormPersistable, classdescriptor As ObjectClassDescription) As iormPersistable
            Dim theKeyvalues As New List(Of Object)
            Dim keyentries As String()

            '** get the keys althoug determining if TOEntries are by Primarykey is a bit obsolete
            If attribute.HasValueToPrimarykeys Then
                keyentries = attribute.ToPrimaryKeys
            ElseIf Not attribute.HasValueFromEntries And attribute.HasValueToEntries Then
                keyentries = attribute.ToEntries
            ElseIf attribute.HasValueFromEntries Then
                keyentries = attribute.FromEntries
            Else
                CoreMessageHandler(message:="relation attribute has nor fromEntries or ToEntries set", _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetObjectByRetrieve", messagetype:=otCoreMessageType.InternalError)
                Return Nothing
            End If

            Try
                Dim aTargetObjectDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(attribute.LinkObject)
                Dim aTargetType As System.Type = aTargetObjectDescriptor.Type
                theKeyvalues = Reflector.GetValues(dataobject:=dataobject, entrynames:=keyentries)
                Dim runtimeOnly As Boolean = CurrentSession.IsBootstrappingInstallationRequested ' only on runtime if we are bootstrapping

                '** full primary key

                Dim retrieveMethod = ot.GetMethodInfo(aTargetType, ObjectClassDescription.ConstMTRetrieve)
                If retrieveMethod IsNot Nothing Then
                    '** relate also in the runtime !
                    Dim anObject As iormPersistable = retrieveMethod.Invoke(Nothing, {theKeyvalues.ToArray, "", Nothing, False, runtimeOnly})
                    Return anObject
                Else
                    CoreMessageHandler(message:="the RETRIEVE method was not found on this object class", messagetype:=otCoreMessageType.InternalError, _
                                        objectname:=aTargetType.Name, subname:="Reflector.GetObjectByRetrieve")
                    Return Nothing
                End If

                '*** return
                Return Nothing

            Catch ex As Exception
                CoreMessageHandler(exception:=ex, _
                                    arg1:=attribute.Name, objectname:=dataobject.ObjectID, _
                                     subname:="Reflector.GetRelatedObjectByRetrieve")
                Return Nothing
            End Try




        End Function


    End Class
End Namespace
