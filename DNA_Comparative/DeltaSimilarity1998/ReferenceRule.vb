﻿#Region "Microsoft.VisualBasic::1978d2d99eb24f61cfa8e32297ca8c58, ..\GCModeller\analysis\SequenceToolkit\DNA_Comparative\DeltaSimilarity1998\ReferenceRule.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.genomics.Assembly.NCBI.GenBank
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.TabularFormat
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports SMRUCC.genomics.ComponentModel.Loci
Imports SMRUCC.genomics.SequenceModel
Imports SMRUCC.genomics.SequenceModel.FASTA
Imports nucl = SMRUCC.genomics.SequenceModel.NucleotideModels.SimpleSegment

Namespace DeltaSimilarity1998

    ''' <summary>
    ''' 生成参考所使用的外标尺核酸片段
    ''' </summary>
    Public Module ReferenceRule

        ''' <summary>
        ''' chromosomal replication initiator protein DnaA
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dnaA As New NamedCollection(Of String) With {
            .Name = "dnaA",
            .Value = {
                "chromosomal replication initiator protein DnaA",
                "chromosomal replication initiator"
            }
        }
        ''' <summary>
        ''' DNA gyrase, B subunit
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property gyrB As New NamedCollection(Of String) With {
            .Name = "gyrB",
            .Value = {
                "DNA gyrase B subunit",
                "DNA gyrase, B subunit"
            }
        }

        ''' <summary>
        ''' Using the DNA segment between the ``dnaA`` and ``gyrB`` as the reference rule.
        ''' </summary>
        ''' <param name="nt"></param>
        ''' <param name="proteins"></param>
        ''' <returns></returns>
        <Extension>
        Public Function dnaA_gyrB(nt As FastaToken, proteins As PTT) As FastaToken
            Return nt.GetReferenceRule(proteins, dnaA, gyrB)
        End Function

        ''' <summary>
        ''' Using the DNA segment between the ``dnaA`` and ``gyrB`` as the reference rule.
        ''' </summary>
        ''' <returns></returns>
        <Extension>
        Public Function dnaA_gyrB(genome As GBFF.File) As FastaToken
            Return genome.GetReferenceRule(dnaA, gyrB)
        End Function

        ''' <summary>
        ''' 获取默认的外标尺：基因组之中的dnaA-gyrB之间的序列
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Get.Ref_Rule",
                   Info:="Gets the segment betweens the dnaA and gyrB nucleotide sequence as the default reference rule for the homogeneity measuring.")>
        <Extension>
        Public Function GetReferenceRule(nt As FastaToken, PTT As PTT, start As NamedCollection(Of String), ends As NamedCollection(Of String)) As FastaToken
            Dim dnaA As GeneBrief = PTT.MatchGene(start.Name, start.Value)
            Dim gyrB As GeneBrief = PTT.MatchGene(ends.Name, ends.Value)

            If (dnaA Is Nothing OrElse gyrB Is Nothing) Then
                Call $"Could not found gene '{start.Name}' or '{ends.Name}' on {nt.Title}".PrintException
                Return Nothing
            End If

            ' 默认dnaA - gyrB这个基因簇是位于正义链的
            Dim St As Integer = dnaA.Location.Left
            Dim sp As Integer = gyrB.Location.Right

            ' 但是有些基因组或者由于测序的原因，位于负义链。。。
            If dnaA.Location.Strand = Strands.Reverse Then
                St = gyrB.Location.Left
                sp = dnaA.Location.Right
            End If

            Dim ruleSegment As nucl
            Try
                ' 构建基因组外标尺片段的计算模型
                ruleSegment = nt.CutSequenceLinear(left:=St, right:=sp)
                If ruleSegment.Length > 10 * 1000 Then
                    Call $"Location exception on (""{nt.Title}"") parsing segment.".PrintException
                    Return Nothing
                End If
            Catch ex As Exception
                Call App.LogException(ex)
                Call ex.PrintException
                Return Nothing
            End Try

            Return New FastaToken With {
                .Attributes = New String() {$"{start.Name}-{ends.Name}", nt.Title},
                .SequenceData = ruleSegment.SequenceData
            }
        End Function

        <Extension>
        Public Function GetReferenceRule(genome As GBFF.File, start As NamedCollection(Of String), ends As NamedCollection(Of String)) As FastaToken
            Dim nt As FastaToken = genome.Origin.ToFasta
            Dim proteins As PTT = genome.GbffToPTT(ORF:=True)
            Return nt.GetReferenceRule(proteins, start, ends)
        End Function
    End Module
End Namespace
