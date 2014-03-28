Option Explicit On

Imports System.Reflection
Imports System.ComponentModel

REM ***********************************************************************************************************************************************''' <summary>
REM *********** ON TRACK DATABASE BACKEND LIBRARY
REM ***********
REM *********** CORE HELPER Classes for On Track Database Backend Library
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
    ''' Converter Class for ORM Data
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Converter

        Public Shared Function SplitFullName(name As String, Optional ByRef part1 As String = Nothing, Optional ByRef part2 As String = Nothing) As Boolean
            Dim names As String() = name.ToUpper.Split({CChar(ConstDelimiter), "."c})
            If names.Count > 1 Then
                part1 = names(0)
                part2 = names(1)
            Else
                part1 = ""
                part2 = name
            End If
            Return True
        End Function

        ''' <summary>
        ''' Converts String to Array
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function String2Array(input As String) As String()
            String2Array = SplitMultbyChar(text:=input, DelimChar:=ConstDelimiter)
            If Not IsArrayInitialized(String2Array) Then
                Return New String() {}
            Else
                Return String2Array
            End If
        End Function
        ''' <summary>
        ''' Converts Array to String
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Array2String(input() As Object) As String
            Dim i As Integer
            If IsArrayInitialized(input) Then
                Dim aStrValue As String = ""
                For i = LBound(input) To UBound(input)
                    If i = LBound(input) Then
                        aStrValue = ConstDelimiter & UCase(input(i).ToString) & ConstDelimiter
                    Else
                        aStrValue = aStrValue & UCase(input(i)) & ConstDelimiter
                    End If
                Next i
                Return aStrValue
            Else
                Return ""
            End If
        End Function
        ''' <summary>
        ''' Converts iEnumerable to String
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Enumerable2String(input As IEnumerable) As String
            Dim aStrValue As String = ""
            If input Is Nothing Then Return ""
            For Each anElement In input
                Dim s As String
                If anElement Is Nothing Then
                    s = ""
                Else
                    s = anElement.ToString
                End If


                If aStrValue = "" Then
                    aStrValue = ConstDelimiter & s & ConstDelimiter
                Else
                    aStrValue &= s & ConstDelimiter
                End If
            Next
            Return aStrValue
        End Function
        ''' <summary>
        ''' converts a string representation of OnTrack DB Type to an object
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="datatype"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function String2DBType(input As String, datatype As otFieldDataType) As Object
            Select Case datatype
                Case otFieldDataType.Bool
                    If input Is Nothing Then
                        Return False
                    ElseIf IsNumeric(input) Then
                        If CLng(input) = 0 Then
                            Return False
                        Else
                            Return True
                        End If
                    ElseIf String.IsNullOrWhiteSpace(input) Then
                        Return False
                    ElseIf input.Trim.ToUpper = "TRUE" OrElse input.Trim.ToUpper = "YES" Then
                        Return True
                    ElseIf input.Trim.ToUpper = "FALSE" OrElse input.Trim.ToUpper = "NO" Then
                        Return False
                    Else
                        Return CBool(input)
                    End If

                Case otFieldDataType.Long
                    If input Is Nothing Then
                        Return CLng(0)
                    ElseIf IsNumeric(input) Then
                        Return CLng(input)
                    Else
                        Return CLng(0)
                    End If

                Case otFieldDataType.Numeric
                    If input Is Nothing Then
                        Return CDbl(0)
                    ElseIf IsNumeric(input) Then
                        Return CDbl(input)
                    Else
                        Return CDbl(0)
                    End If
                Case otFieldDataType.List
                    Return ConstDelimiter & ConstDelimiter
                Case otFieldDataType.Memo, otFieldDataType.Text
                    If input Is Nothing Then
                        Return ""
                    Else
                        Return input
                    End If

                Case otFieldDataType.Date, otFieldDataType.Timestamp
                    If input Is Nothing OrElse Not IsDate(input) Then
                        Return ConstNullDate
                    Else
                        Return CDate(input)
                    End If

                Case otFieldDataType.Time
                    If input Is Nothing OrElse Not IsDate(input) Then
                        Return ConstNullTime
                    Else
                        Return CDate(input)
                    End If

                Case Else
                    Return Nothing
            End Select

        End Function
    End Class
    ''' <summary>
    ''' Reflector Class for reflecting ORM Attributes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Reflector
        ''' <summary>
        ''' returns true if the type is nullable
        ''' </summary>
        ''' <param name="myType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsNullableTypeOrString(ByVal [type] As Type) As Boolean
            Return ([type] Is GetType(String)) OrElse ([type].IsGenericType) AndAlso ([type].GetGenericTypeDefinition() Is GetType(Nullable(Of )))
        End Function

        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAttributes(ormType As Type) As List(Of System.Attribute)
            Dim aFieldList As System.Reflection.FieldInfo()
            Dim anAttributeList As New List(Of System.Attribute)

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            '** TABLE
                            If anAttribute.GetType().Equals(GetType(ormSchemaTableAttribute)) Then
                                '* set the tablename
                                DirectCast(anAttribute, ormSchemaTableAttribute).TableName = aFieldInfo.GetValue(Nothing).ToString
                                anAttributeList.Add(anAttribute)
                                '** FIELD COLUMN
                            ElseIf anAttribute.GetType().Equals(GetType(ormObjectEntryAttribute)) Then
                                '* set the cloumn name
                                DirectCast(anAttribute, ormObjectEntryAttribute).ColumnName = aFieldInfo.GetValue(Nothing).ToString

                                anAttributeList.Add(anAttribute)
                                '** INDEX
                            ElseIf anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                '* set the index name
                                DirectCast(anAttribute, ormSchemaIndexAttribute).IndexName = aFieldInfo.GetValue(Nothing).ToString

                                anAttributeList.Add(anAttribute)
                            End If
                        Next
                    End If
                Next

                Return anAttributeList

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetAttribute", exception:=ex)
                Return anAttributeList

            End Try


        End Function



        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetColumnAttribute(ormType As Type, columnName As String) As System.Attribute
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' Column
                            If anAttribute.GetType().Equals(GetType(ormObjectEntryAttribute)) Then
                                If aFieldInfo.GetValue(Nothing).ToString.ToUpper = columnName.ToUpper Then
                                    '* set the column name
                                    DirectCast(anAttribute, ormObjectEntryAttribute).ColumnName = aFieldInfo.GetValue(Nothing).ToString

                                    Return anAttribute
                                End If
                            End If
                        Next
                    End If
                Next

                Return Nothing

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetColumnAttribute", exception:=ex)
                Return Nothing

            End Try


        End Function


        ''' <summary>
        ''' returns ORM Attributes out of a Type
        ''' </summary>
        ''' <param name="ormType"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetIndexAttribute(ormType As Type, indexName As String) As System.Attribute
            Dim aFieldList As System.Reflection.FieldInfo()

            Try
                '***
                '*** collect all the attributes first
                '***
                aFieldList = ormType.GetFields(Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic Or _
                                                  Reflection.BindingFlags.Public Or Reflection.BindingFlags.Static Or _
                                                  Reflection.BindingFlags.FlattenHierarchy)
                '** look into each  Type (Fields)
                For Each aFieldInfo As System.Reflection.FieldInfo In aFieldList

                    If aFieldInfo.MemberType = Reflection.MemberTypes.Field Then
                        '** Attributes
                        For Each anAttribute As System.Attribute In Attribute.GetCustomAttributes(aFieldInfo)
                            ''' Index
                            If anAttribute.GetType().Equals(GetType(ormSchemaIndexAttribute)) Then
                                If aFieldInfo.GetValue(Nothing).ToString.ToUpper = indexName.ToUpper Then
                                    '* set the index name
                                    DirectCast(anAttribute, ormSchemaIndexAttribute).IndexName = aFieldInfo.GetValue(Nothing).ToString

                                    Return anAttribute
                                End If
                            End If
                        Next
                    End If
                Next

                Return Nothing

            Catch ex As Exception

                Call CoreMessageHandler(subname:="Reflector.GetIndexAttribute", exception:=ex)
                Return Nothing

            End Try

        End Function
        ''' <summary>
        ''' retrieves a list of related objects from a relation attribute for a object class described by a classdescriptor
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="classdescriptor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetValues(dataobject As iormPersistable, Optional entrynames As String() = Nothing) As List(Of Object)
            Dim aDescriptor As ObjectClassDescription = ot.GetObjectClassDescription(dataobject.GetType)
            Dim aList As New List(Of Object)
            If aDescriptor Is Nothing Then
                CoreMessageHandler(message:="Class Description not found for data object", arg1:=dataobject.GetType.Name, _
                                   subname:="Reflector.Getvalues", messagetype:=otCoreMessageType.InternalError)
                Return aList
            End If
            If entrynames Is Nothing Then
                entrynames = aDescriptor.Entrynames.ToArray
            End If

            '*** get the values in the order of the entrynames
            For Each anEntryname In entrynames
                Dim anObjectEntry = aDescriptor.GetObjectEntryAttribute(entryname:=anEntryname)
                If anObjectEntry IsNot Nothing AndAlso anObjectEntry.HasValueColumnName AndAlso anObjectEntry.HasValueTableName Then
                    Dim aFieldlist = aDescriptor.GetMappedColumnFieldInfos(columnname:=anObjectEntry.ColumnName, _
                                                                           tablename:=anObjectEntry.Tablename)
                    If aFieldlist IsNot Nothing AndAlso aFieldlist.Count > 0 Then
                        Dim aValue As Object
                        '** get value by hook or slooow
                        If Not Reflector.GetFieldValue(aFieldlist.First, dataobject, aValue) Then
                            aValue = aFieldlist.First.GetValue(dataobject)
                        End If

                        aList.Add(aValue)
                    ElseIf aFieldlist Is Nothing Then
                        CoreMessageHandler(message:="Object Entry not mapped to a FieldMember of the class ", _
                                       arg1:=dataobject.GetType.Name, entryname:=anEntryname, objectname:=dataobject.ObjectID, _
                                       subname:="Reflector.Getvalues", messagetype:=otCoreMessageType.InternalWarning)
                    Else
                        CoreMessageHandler(message:="Object Entry mapped to multiple FieldMember of the class - first one taken ", _
                                       arg1:=dataobject.GetType.Name, entryname:=anEntryname, objectname:=dataobject.ObjectID, _
                                       subname:="Reflector.Getvalues", messagetype:=otCoreMessageType.InternalWarning)
                    End If

                ElseIf anObjectEntry Is Nothing Then
                    CoreMessageHandler(message:="Object Entry not found in Class Description ", _
                                       arg1:=dataobject.GetType.Name, entryname:=anEntryname, objectname:=dataobject.ObjectID, _
                                       subname:="Reflector.Getvalues", messagetype:=otCoreMessageType.InternalError)
                ElseIf Not anObjectEntry.HasValueColumnName OrElse Not anObjectEntry.HasValueTableName Then
                    CoreMessageHandler(message:="Class Description Object Entry has no tablename or columnname ", _
                                       arg1:=dataobject.GetType.Name, entryname:=anEntryname, objectname:=dataobject.ObjectID, _
                                       subname:="Reflector.Getvalues", messagetype:=otCoreMessageType.InternalError)
                End If
            Next

            Return aList
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
                    domainBehavior = anObjectClassdDescription.ObjectAttribute.AddDomainBehaviorFlag
                    deletebehavior = anObjectClassdDescription.ObjectAttribute.DeleteFieldFlag

                    '** normal way
                Else
                    Dim anObjectDefinition As ObjectDefinition = ot.CurrentSession.Objects.GetObject(objectname:=aTargetObjectDescriptor.ID)
                    domainBehavior = anObjectDefinition.DomainBehavior
                    deletebehavior = anObjectDefinition.DeletePerFlagBehavior
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
        ''' set the member field value with conversion of a dataobject
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="dataobject"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function SetFieldValue(field As FieldInfo, dataobject As iormPersistable, value As Object) As Boolean

            Try
                Dim converter As TypeConverter = TypeDescriptor.GetConverter(field.FieldType)
                Dim aClassDescription = dataobject.ObjectClassDescription 'ot.GetObjectClassDescription(dataobject.GetType)
                If aClassDescription Is Nothing Then
                    CoreMessageHandler(message:="class description of object could not be retrieved", objectname:=dataobject.ObjectID, arg1:=field.Name, _
                                       subname:="Reflector.SetFieldValue", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                Dim aSetter = aClassDescription.GetFieldMemberSetterDelegate(field.Name)
                If aSetter Is Nothing Then
                    CoreMessageHandler(message:="setter delegate of object could not be retrieved - field.setvalue will be used", objectname:=dataobject.ObjectID, arg1:=field.Name, _
                                       subname:="Reflector.SetFieldValue", messagetype:=otCoreMessageType.InternalError)
                End If

                'SyncLock dataobject
                If field.Name = "_typeid" Then
                    ' Debug.Assert(False)
                    Console.Write("")
                End If
                ' do nothing leave the value
                If field.FieldType.IsArray Then
                    Dim anArray As String()
                    If value.GetType.IsArray Then
                        anArray = value
                    Else
                        anArray = OnTrack.Database.Converter.String2Array(value)
                    End If

                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, anArray)
                    Else
                        field.SetValue(dataobject, anArray)
                    End If
                ElseIf field.FieldType.GetInterfaces.Contains(GetType(IList)) Then
                    Dim anArray As String()
                    If value.GetType.IsArray Then
                        anArray = value
                    Else
                        anArray = OnTrack.Database.Converter.String2Array(value)
                    End If

                    Dim aList = anArray.ToList
                    If anArray.Count = 0 Then
                        aList = New List(Of String) 'HACK ! this should be of generic type of the field
                    End If
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, aList)
                    Else
                        field.SetValue(dataobject, aList)
                    End If
                ElseIf value Is Nothing OrElse field.FieldType.Equals(value.GetType) Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, value)
                    Else
                        field.SetValue(dataobject, value)
                    End If

                ElseIf converter.GetType.Equals(GetType(EnumConverter)) Then
                    Dim anewValue As Object
                    If value.GetType.Equals(GetType(String)) Then
                        '* transform
                        anewValue = CTypeDynamic([Enum].Parse(field.FieldType, value, ignoreCase:=True), field.FieldType)
                    Else
                        anewValue = CTypeDynamic(value, field.FieldType)
                    End If

                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, anewValue)
                    Else
                        field.SetValue(dataobject, anewValue)
                    End If
                ElseIf converter.CanConvertFrom(value.GetType) Then
                    Dim anewvalue As Object = converter.ConvertFrom(value)
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, anewvalue)
                    Else
                        field.SetValue(dataobject, anewvalue)
                    End If
                ElseIf field.FieldType.Equals(GetType(Long)) AndAlso IsNumeric(value) Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CLng(value))
                    Else
                        field.SetValue(dataobject, CLng(value))
                    End If
                ElseIf field.FieldType.Equals(GetType(Boolean)) Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CBool(value))
                    Else
                        field.SetValue(dataobject, CBool(value))
                    End If

                ElseIf field.FieldType.Equals(GetType(String)) Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CStr(value))
                    Else
                        field.SetValue(dataobject, CStr(value))
                    End If
                    field.SetValue(dataobject, CStr(value))
                ElseIf field.FieldType.Equals(GetType(Integer)) AndAlso IsNumeric(value) Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CInt(value))
                    Else
                        field.SetValue(dataobject, CInt(value))
                    End If

                ElseIf field.FieldType.Equals(GetType(UInteger)) AndAlso IsNumeric(value) _
                    AndAlso value >= UInteger.MinValue AndAlso value <= UInteger.MaxValue Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CUInt(value))
                    Else
                        field.SetValue(dataobject, CUInt(value))
                    End If
                ElseIf field.FieldType.Equals(GetType(UShort)) And IsNumeric(value) _
                    AndAlso value >= UShort.MinValue AndAlso value <= UShort.MaxValue Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CUShort(value))
                    Else
                        field.SetValue(dataobject, CUShort(value))
                    End If
                ElseIf field.FieldType.Equals(GetType(ULong)) And IsNumeric(value) _
                     AndAlso value >= ULong.MinValue AndAlso value <= ULong.MaxValue Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CULng(value))
                    Else
                        field.SetValue(dataobject, CULng(value))
                    End If

                ElseIf field.FieldType.Equals(GetType(Double)) And IsNumeric(value) _
                    AndAlso value >= Double.MinValue AndAlso value <= Double.MaxValue Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CDbl(value))
                    Else
                        field.SetValue(dataobject, CDbl(value))
                    End If
                ElseIf field.FieldType.Equals(GetType(Decimal)) And IsNumeric(value) _
                  AndAlso value >= Decimal.MinValue AndAlso value <= Decimal.MaxValue Then
                    If aSetter IsNot Nothing Then
                        aSetter(dataobject, CDec(value))
                    Else
                        field.SetValue(dataobject, CDec(value))
                    End If
                Else
                    Call CoreMessageHandler(subname:="ormDataObject.infuse", message:="cannot convert record value type to field type", _
                                           entryname:=field.Name, tablename:=dataobject.TableID, _
                                           arg1:=field.Name, messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                'End SyncLock

                Return True

            Catch ex As Exception

                CoreMessageHandler(exception:=ex, subname:="Reflector.SetFieldValue", arg1:=value, entryname:=field.Name, objectname:=dataobject.ObjectID)
                Return False
            End Try


        End Function
        ''' <summary>
        ''' set the member field value with conversion of a dataobject
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="dataobject"></param>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetFieldValue(field As FieldInfo, dataobject As iormPersistable, ByRef value As Object) As Boolean

            Try
                'Dim converter As TypeConverter = TypeDescriptor.GetConverter(field.FieldType)
                Dim aClassDescription = dataobject.ObjectClassDescription 'ot.GetObjectClassDescription(dataobject.GetType)
                If aClassDescription Is Nothing Then
                    CoreMessageHandler(message:="class description of object could not be retrieved", objectname:=dataobject.ObjectID, arg1:=field.Name, _
                                       subname:="Reflector.GetFieldValue", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If
                Dim aGetter = aClassDescription.GetFieldMemberGetterDelegate(field.Name)
                If aGetter Is Nothing Then
                    CoreMessageHandler(message:="setter delegate of object could not be retrieved", objectname:=dataobject.ObjectID, arg1:=field.Name, _
                                      subname:="Reflector.GetFieldValue", messagetype:=otCoreMessageType.InternalError)
                    Return False
                End If

                value = aGetter(dataobject)

                Return True

            Catch ex As Exception

                CoreMessageHandler(exception:=ex, subname:="Reflector.GetFieldValue", arg1:=value, entryname:=field.Name, objectname:=dataobject.ObjectID)
                Return False
            End Try


        End Function
    End Class

End Namespace
