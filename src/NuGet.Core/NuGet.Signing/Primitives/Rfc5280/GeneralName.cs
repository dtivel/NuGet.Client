// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Signing.Primitives.Rfc5280
{
    /*
        // RFC 5280 section 4.2.1.6 (https://tools.ietf.org/html/rfc5280#section-4.2.1.6)
        GeneralName ::= CHOICE {
            otherName                       [0]     OtherName,
            rfc822Name                      [1]     IA5String,
            dNSName                         [2]     IA5String,
            x400Address                     [3]     ORAddress,
            directoryName                   [4]     Name,
            ediPartyName                    [5]     EDIPartyName,
            uniformResourceIdentifier       [6]     IA5String,
            iPAddress                       [7]     OCTET STRING,
            registeredID                    [8]     OBJECT IDENTIFIER }

        https://tools.ietf.org/html/rfc5280#section-4.1.2.4
       Name ::= CHOICE { -- only one possibility for now --
         rdnSequence  RDNSequence }

       RDNSequence ::= SEQUENCE OF RelativeDistinguishedName

       RelativeDistinguishedName ::=
         SET SIZE (1..MAX) OF AttributeTypeAndValue

       AttributeTypeAndValue ::= SEQUENCE {
         type     AttributeType,
         value    AttributeValue }

       AttributeType ::= OBJECT IDENTIFIER

       AttributeValue ::= ANY -- DEFINED BY AttributeType

       DirectoryString ::= CHOICE {
             teletexString           TeletexString (SIZE (1..MAX)),
             printableString         PrintableString (SIZE (1..MAX)),
             universalString         UniversalString (SIZE (1..MAX)),
             utf8String              UTF8String (SIZE (1..MAX)),
             bmpString               BMPString (SIZE (1..MAX)) }

        -- X.400 address syntax starts here
    ORAddress ::= SEQUENCE {
       built-in-standard-attributes BuiltInStandardAttributes,
       built-in-domain-defined-attributes
                       BuiltInDomainDefinedAttributes OPTIONAL,
       -- see also teletex-domain-defined-attributes
       extension-attributes ExtensionAttributes OPTIONAL }
    */
    public sealed class GeneralName
    {
        public OtherName OtherName { get; }
        public byte[] Rfc822Name { get; }
        public byte[] DnsName { get; }
        public object X400Address { get; } // TODO
        public object DirectoryName { get; } // TODO
        public EdiPartyName EdiPartyName { get; }
        public byte[] UniformResourceIdentifier { get; }
        public byte[] IpAddress { get; }
        public string RegisteredID { get; }
    }
}