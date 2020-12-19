using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Elastic_CRUD.View
{
    public partial class Form_CRUD : Form
    {

        #region [[ Properties ]]

        private BLL.Customer _customerBll;

        #endregion

        #region [[ Constructors ]]
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Form_CRUD()
        {
            InitializeComponent();
        }

        #endregion

        #region [[ Methods ]]


        /// <summary>
        /// Fill the ListView with Elastic return
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private void FillListView(List<DTO.Customer> list, ListView component)
        {
            BuildListView(component);

            for (int i = 0; i < list.Count; i++)
            {
                ListViewItem lvwItem = new ListViewItem();
                lvwItem.Text = list[i].Id.ToString();
                lvwItem.SubItems.Add(list[i].Name);
                lvwItem.SubItems.Add(list[i].Age.ToString());
                lvwItem.SubItems.Add(list[i].HasChildren.ToString());
                lvwItem.SubItems.Add(list[i].EnrollmentFee.ToString("C"));
                lvwItem.SubItems.Add(list[i].Opinion);
                component.Items.Add(lvwItem);
            }

        }

        /// <summary>
        /// Setup the List View 
        /// </summary>
        private void BuildListView(ListView component)
        {
            component.Columns.Clear();
            component.Items.Clear();

            component.View = System.Windows.Forms.View.Details;
            component.FullRowSelect = true;
            component.GridLines = true;

            component.Columns.Add("Id", 50);
            component.Columns.Add("Name", 100);
            component.Columns.Add("Age", 50);
            component.Columns.Add("Has Children", 100);
            component.Columns.Add("Enrollment Fee", 100);
            component.Columns.Add("Opinion", 100);
        }

        /// <summary>
        /// Setup the List View - For aggregations
        /// </summary>
        private void BuildAggregationListView(ListView component)
        {
            component.Columns.Clear();
            component.Items.Clear();

            component.View = System.Windows.Forms.View.Details;
            component.FullRowSelect = true;
            component.GridLines = true;

            component.Columns.Add("Bucket", 150);
            component.Columns.Add("Value", 100);
        }

        /// <summary>
        /// Fill the ListView with Elastic return - For aggregations
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private void FillAggregationListView(Dictionary<string, double> list, ListView component)
        {
            BuildAggregationListView(component);

            foreach (var item in list.Keys)
            {
                ListViewItem lvwItem = new ListViewItem();
                lvwItem.Text = item;
                lvwItem.SubItems.Add(list[item].ToString());
                component.Items.Add(lvwItem); 
            }

        }

        /// <summary>
        /// Create Entity for searching
        /// </summary>
        /// <returns></returns>
        private DTO.Customer CreateEntityForSearching()
        {
            //TODO: Validation
            DTO.Customer entity = new DTO.Customer();            

            entity.Age = int.Parse(nudQryAge.Value.ToString());
            entity.Birthday = dtpQryBirthday.Value.ToString(DTO.Constants.BASIC_DATE);                        
            entity.EnrollmentFee = double.Parse(mskQryEnrol.Text);
            entity.HasChildren = bool.Parse(cboQryChildren.Text);
            entity.Name = txtQryName.Text;
            entity.Id = int.Parse(mskQryId.Text);
            return entity;
        }

        #endregion

        
        #region [[ Events ]]

        /// <summary>
        /// Foram load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_CRUD_Load(object sender, EventArgs e)
        {
            try
            {
                _customerBll = new BLL.Customer();
                mskEnrollmentFee.TextMaskFormat = MaskFormat.IncludeLiterals;
                mskId.TextMaskFormat = MaskFormat.IncludeLiterals;
                cboHasChildren.SelectedText = "false";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            
        }

        /// <summary>
        /// Saving into Elastic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //TODO Validation
                DTO.Customer entity = new DTO.Customer();

                entity.Age = int.Parse(nudAge.Value.ToString());
                entity.Birthday = dtpBirthday.Value.ToString(DTO.Constants.BASIC_DATE);
                entity.EnrollmentFee = double.Parse(mskEnrollmentFee.Text);
                entity.HasChildren = bool.Parse(cboHasChildren.Text);
                entity.Name = txtName.Text;
                entity.Opinion = txtOpnion.Text;
                entity.Id = int.Parse(mskId.Text);

                if( _customerBll.Index(entity) )
                    MessageBox.Show("Index saved!", "Ok", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        /// Deleting from Elastic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleting_Click(object sender, EventArgs e)
        {
            try
            {
                if ( _customerBll.Delete(mskId.Text) )
                    MessageBox.Show("Index deleted!", "Ok", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Index wasn't deleted!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);                
            }
        }

        /// <summary>
        /// Running query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQrt1_Click(object sender, EventArgs e)
        {
            try
            {
                FillListView(_customerBll.QueryById(mskQryId.Text), lvwHits);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///  Running query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQrt2_Click(object sender, EventArgs e)
        {
            try
            {
                DTO.Customer entity = CreateEntityForSearching();

                FillListView(_customerBll.QueryByAllFieldsUsingAnd(entity), lvwHits);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

         /// <summary>
         ///  Running query
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void btnQrt3_Click(object sender, EventArgs e)
        {
            try
            {
                DTO.Customer entity = CreateEntityForSearching();

                FillListView(_customerBll.QueryByAllFieldsUsingOr(entity), lvwHits);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Running query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQryComb1_Click(object sender, EventArgs e)
        {
            try
            {
                DTO.CombinedFilter filter = new DTO.CombinedFilter();
                //TODO: Validation
                filter.HasChildren = bool.Parse(cboQryMustHasChildren.Text);
                filter.Ages = txtQryShouldHaveAges.Text.Split(',').ToList<string>();
                filter.Names = txtQryMustNotName.Text.Split(',').ToList<string>();

                FillListView(_customerBll.QueryUsingCombinations(filter), lvwHits2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///  Running query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQryRange1_Click(object sender, EventArgs e)
        {
            try
            {
                DTO.RangeFilter range = new DTO.RangeFilter();
                //TODO: Validation                
                range.Birthday = dtpQryRangeDate.Value;
                range.EnrollmentFeeStart = double.Parse(mskQryRangeEnrol1.Text);
                range.EnrollmentFeeEnd = double.Parse(mskQryRangeEnrol2.Text);

                FillListView(_customerBll.QueryUsingRanges(range), lvwHits3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

         /// <summary>
         ///  Running aggregations
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void btnRunAgrr_Click(object sender, EventArgs e)
        {
            try
            {
                //TODO: validation  (eg: Sum is not applicable to 'name' field)
                DTO.Aggregations aggFilter = new DTO.Aggregations();
                aggFilter.Field = cboQryField.Text;
                aggFilter.AggregationType = cboQryAggrType.Text;

                FillAggregationListView( _customerBll.GetAggregations(aggFilter), lvwHits4);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        #endregion

    }
}
