using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWBatchBackup.SQLObjectFilter
{
    public class ObjectSelector
    {
        private SQLObjectType _Assemblies;
        private SQLObjectType _PartitionFunctions;
        private SQLObjectType _PartitionSchemes;
        private SQLObjectType _Roles;
        private SQLObjectType _Schemas;
        private SQLObjectType _SchemaCollections;
        private SQLObjectType _StoredProcedures;
        private SQLObjectType _Synonyms;
        private SQLObjectType _Tables;
        private SQLObjectType _Triggers;
        private SQLObjectType _UserDefinedFunctions;
        private SQLObjectType _UserDefinedDataTypes;
        private SQLObjectType _UserDefinedTableTypes;
        private SQLObjectType _Views;

        public SQLObjectType Assemblies
        {
            get
            {
                if (_Assemblies == null)
                {
                    _Assemblies = new SQLObjectType();
                }
                return _Assemblies;
            }

            set
            {
                _Assemblies = value;
            }
        }

        public SQLObjectType PartitionFunctions
        {
            get
            {
                if (_PartitionFunctions == null)
                {
                    _PartitionFunctions = new SQLObjectType();
                }
                return _PartitionFunctions;
            }

            set
            {
                _PartitionFunctions = value;
            }
        }

        public SQLObjectType PartitionSchemes
        {
            get
            {
                if (_PartitionSchemes == null)
                {
                    _PartitionSchemes = new SQLObjectType();
                }
                return _PartitionSchemes;
            }

            set
            {
                _PartitionSchemes = value;
            }
        }

        public SQLObjectType Roles
        {
            get
            {
                if (_Roles == null)
                {
                    _Roles = new SQLObjectType();
                }
                return _Roles;
            }

            set
            {
                _Roles = value;
            }
        }

        public SQLObjectType Views
        {
            get
            {
                if (_Views == null)
                {
                    _Views = new SQLObjectType();
                }
                return _Views;
            }

            set
            {
                _Views = value;
            }
        }

        public SQLObjectType UserDefinedFunctions
        {
            get
            {
                if (_UserDefinedFunctions == null)
                {
                    _UserDefinedFunctions = new SQLObjectType();
                }
                return _UserDefinedFunctions;
            }

            set
            {
                _UserDefinedFunctions = value;
            }
        }

        public SQLObjectType UserDefinedDataTypes
        {
            get
            {
                if (_UserDefinedDataTypes == null)
                {
                    _UserDefinedDataTypes = new SQLObjectType();
                }
                return _UserDefinedDataTypes;
            }

            set
            {
                _UserDefinedDataTypes = value;
            }
        }

        public SQLObjectType UserDefinedTableTypes
        {
            get
            {
                if (_UserDefinedTableTypes == null)
                {
                    _UserDefinedTableTypes = new SQLObjectType();
                }
                return _UserDefinedTableTypes;
            }

            set
            {
                _UserDefinedTableTypes = value;
            }
        }

        public SQLObjectType StoredProcedures
        {
            get
            {
                if (_StoredProcedures == null)
                {
                    _StoredProcedures = new SQLObjectType();
                }
                return _StoredProcedures;
            }

            set
            {
                _StoredProcedures = value;
            }
        }

        public SQLObjectType Triggers
        {
            get
            {
                if (_Triggers == null)
                {
                    _Triggers = new SQLObjectType();
                }
                return _Triggers;
            }

            set
            {
                _Triggers = value;
            }
        }

        public SQLObjectType Schemas
        {
            get
            {
                if (_Schemas == null)
                {
                    _Schemas = new SQLObjectType();
                }
                return _Schemas;
            }

            set
            {
                _Schemas = value;
            }
        }

        public SQLObjectType SchemaCollections
        {
            get
            {
                if (_SchemaCollections == null)
                {
                    _SchemaCollections = new SQLObjectType();
                }
                return _SchemaCollections;
            }

            set
            {
                _SchemaCollections = value;
            }
        }

        public SQLObjectType Synonyms
        {
            get
            {
                if (_Synonyms == null)
                {
                    _Synonyms = new SQLObjectType();
                }
                return _Synonyms;
            }

            set
            {
                _Synonyms = value;
            }
        }

        public SQLObjectType Tables
        {
            get
            {
                if (_Tables == null)
                {
                    _Tables = new SQLObjectType();
                }
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }
    }
}
