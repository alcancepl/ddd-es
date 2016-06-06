using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.ValidateObject.Tests
{
    [TestClass()]
    public class ValidateObjectAttributeTests
    {
        [TestMethod()]
        public void ValidateObjectAttributeValidatesInvalidSubChildModel()
        {
            var model = new Model
            {
                RequiredOne = "000",
                ChildModel = new ChildModel
                {
                    RequiredValue = "111",
                    SubChildModel = new SubChildModel
                    {
                        RequiredValue = "222"
                    },
                    SubChildModelList = new List<SubChildModel>
                    {
                    }
                },
                ChildModelList = new List<ChildModel> {
                    new ChildModel {
                        RequiredValue = "111",
                        SubChildModel = new SubChildModel {
                            RequiredValue = "222"
                        },
                        SubChildModelList = new List<SubChildModel> {
                            new SubChildModel {
                                RequiredValue = "333"
                            },
                            new SubChildModel {
                                RequiredValue = "" // Missing required property!
                            }
                        }
                    },
                    new ChildModel {
                        RequiredValue = "111",
                        SubChildModel = new SubChildModel {
                            RequiredValue = "222"
                        },
                        SubChildModelList = new List<SubChildModel> {
                            new SubChildModel {
                                RequiredValue = "333"
                            },
                            new SubChildModel {
                                RequiredValue = "333"
                            }
                        }
                    }
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            Assert.IsFalse(ok);
        }

        [TestMethod()]
        public void ValidateObjectAttributeValidatesValidSubChildModel()
        {
            var model = new Model
            {
                RequiredOne = "000",
                ChildModel = new ChildModel
                {
                    RequiredValue = "111",
                    SubChildModel = new SubChildModel
                    {
                        RequiredValue = "222"
                    },
                    SubChildModelList = new List<SubChildModel>
                    {
                    }
                },
                ChildModelList = new List<ChildModel> {
                    new ChildModel {
                        RequiredValue = "111",
                        SubChildModel = new SubChildModel {
                            RequiredValue = "222"
                        },
                        SubChildModelList = new List<SubChildModel> {
                            new SubChildModel {
                                RequiredValue = "333"
                            },
                            new SubChildModel {
                                RequiredValue = "333"
                            }
                        }
                    },
                    new ChildModel {
                        RequiredValue = "111",
                        SubChildModel = new SubChildModel {
                            RequiredValue = "222"
                        },
                        SubChildModelList = new List<SubChildModel> {
                            new SubChildModel {
                                RequiredValue = "333"
                            },
                            new SubChildModel {
                                RequiredValue = "333"
                            }
                        }
                    }
                }
            };
            List<ValidationResult> results = new List<ValidationResult>();
            var ok = Validator.TryValidateObject(model, new ValidationContext(model), results, true);
            Assert.IsTrue(ok);
        }
    }
}