/*
Copyright (c) 2015 Eric Begue (ericbeg@gmail.com)

This source file is part of the Panda BT package, which is licensed under
the Unity's standard Unity Asset Store End User License Agreement ("Unity-EULA").

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace PandaBT.Runtime
{

	public abstract class BTProbalisticNode : BTCompositeNode
	{
		static System.Random rand = new System.Random();
		public delegate float RandomValueDelegate();
		static public RandomValueDelegate randomValue = () => (float)rand.NextDouble();


		protected bool areValidWeights = false;
		internal BTNode m_selectedChild = null;

		protected List<BTNode> m_uncompletedChildren = new List<BTNode>();

		struct IndexWeight
		{
			public int index;
			public float weight;
		}

		IndexWeight[] indexWeights = null;
		public BTProbalisticNode()
			: base()
		{
			
		}
		
	
		public BTProbalisticNode( params BTNode[] children)
			: base( children )
		{
			
		}

		protected void ProcessWeights()
		{
			var children = this.children;

			indexWeights = new IndexWeight[children.Length];

			List<int> noWeights = new List<int>();

			int countWeighted = 0;
			for(int i = 0; i < children.Length; i++ )
			{
				indexWeights[i].index = i;
				indexWeights[i].weight = 0.0f;

				if( i < ParameterValues.Length )
				{
					float weight = 0.0f;
					
					if( children[i].status == Status.Ready)
					{
						if (ParameterValues[i].GetType() == typeof(float))
							weight = (float)ParameterValues[i];

						if (ParameterValues[i].GetType() == typeof(int))
							weight = System.Convert.ToSingle((int)ParameterValues[i]);
					}

					indexWeights[i].weight = weight;
					countWeighted++;
				}
				else
				{
					noWeights.Add(i);
				}
			}


			float AWeight = 0.0f;
			for(int i = 0; i < children.Length; i++ )
				AWeight += indexWeights[i].weight;

			float a = (AWeight > 0.0f || countWeighted==children.Length) ? (float)countWeighted / ((float)children.Length): 0.0f;
			float b  = 1.0f-a;

			float Bweight = a > 0.0f?(AWeight*b)/a: 1.0f;

			areValidWeights = AWeight + Bweight > 0.0f;

			float w = (noWeights.Count >0 ) ? Bweight/( (float)noWeights.Count  ): 0.0f;


			foreach (int i in noWeights)
				indexWeights[i].weight = children[i].status == Status.Ready ? w : 0.0f;

			float sum = AWeight + Bweight;
			for (int i = 0; i < children.Length; i++)
			{
				indexWeights[i].weight /= sum;

				if (indexWeights[i].weight < 0.0f)
					areValidWeights = false;
			}


			// sort index by weight
			var wl = new List<IndexWeight>( indexWeights );
			wl.Sort(delegate(IndexWeight left, IndexWeight right) { return left.weight.CompareTo(right.weight); });

			indexWeights = wl.ToArray();

		}

		protected override void DoReset()
		{
			base.DoReset();

			ProcessWeights();

			m_selectedChild = null;

			m_uncompletedChildren.Clear();
			m_uncompletedChildren.AddRange(this.children);

		}

		protected void SelectNextChildToRun()
		{

			if( areValidWeights == false)
			{
				if( m_uncompletedChildren.Count > 0)
				{
					m_selectedChild = m_uncompletedChildren.FirstOrDefault();
				}
				return;
			}


			int runningIdx = 0;

			float r = randomValue();
			float acum = 0.0f;

			// 0.0f                                     1.0f
			//  |----|--|-----|... |-----|  ... |----|---|   
			//    0    1    2         i           n-2   n-1
			//                      ^
			//                      r

			var candidate = this.children;
			for (int j = 0; j < candidate.Length; j++)
			{
				acum += indexWeights[j].weight;
				if (r <= acum)
				{
					runningIdx = indexWeights[j].index;
					m_selectedChild = candidate[runningIdx];
					break;
				}
			}
		}

		internal override BTNodeState state
		{
			get
			{
				// return new BTRandomState(this);
				throw new System.Exception("Not implemented");
			}

			set
			{
				var _state = value as BTRandomState;
				base.state = _state;
				m_selectedChild = children[_state.selectedChildIndex];
			}
		}

	}

}

