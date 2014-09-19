using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;


namespace TheTunnel
{
	public class Communicator
	{
		public Communicator (CordDispatcher dispatcher)
		{
			this.Dispatcher = dispatcher;
		}

		public Func<Tanswer, Tquestion> GetResponder<Tanswer, Tquestion>(string questionCordName, string answerCordName)
		{
			var cord = Dispatcher.GetCord (questionCordName) as AskingProtocord<Tanswer, Tquestion>;
			if (cord != null && cord.AnswerCord.Name != answerCordName) {
				Dispatcher.RemoveCord (questionCordName);
				Dispatcher.RemoveCord (answerCordName);
				cord = null;
			}
			if(cord==null)
			{
			    cord = new AskingProtocord<Tanswer,Tquestion> (questionCordName, answerCordName);
				Dispatcher.AddCord (cord);
			}
			Func<Tanswer, Tquestion> res =  ((arg) => cord.Ask (arg, 10000));
		    return res;
		}

		public void SetResponder<Tanswer, Tquestion>(string questionCordName, string answerCordName, Func<Tanswer, Tquestion> responder)
		{
			var cord = Dispatcher.GetCord (questionCordName) as AskingProtocord<Tanswer, Tquestion>;
			if (cord != null && cord.AnswerCord.Name != answerCordName) {
				Dispatcher.RemoveCord (questionCordName);
				Dispatcher.RemoveCord (answerCordName);
				cord = null;
			}
			if (cord == null) {
				cord = new AskingProtocord<Tanswer,Tquestion> (questionCordName, answerCordName);
				Dispatcher.AddCord (cord);
			} 
			cord.OnAsking += (ICord sender, Tanswer ans) => responder(ans);
		}

		public Action<T> GetSpeaker<T>(string sayCordName) 
		{
			var cord = Dispatcher.GetCord (sayCordName) as SayingProtocord<T>;
			if(cord==null)
			{
				cord = new SayingProtocord<T> (sayCordName);
				Dispatcher.AddCord (cord);
			}
			return cord.Send;
		}

		public void SetListener<T>(string sayCordName, Action<T> listener)
		{
			var cord = Dispatcher.GetCord (sayCordName) as SayingProtocord<T>;
			if(cord==null)
			{
				cord = new SayingProtocord<T> (sayCordName);
				Dispatcher.AddCord (cord);
			}
			cord.OnReceiveT+= (sender, msg) => listener(msg);
		}

	
		public CordDispatcher Dispatcher{ get; protected set;}


		public void SetCordContract(ICordContract cordContract)
		{
			//HELL
			//
			//IS
			//
			//HERE!
			var type = cordContract.GetType ();

			var ReceiveCords = type
				.GetMethods ()
				.Select (m => new 
					{
						method = m, 
						attr = m.GetCustomAttributes (typeof(ReceiveAttribute), true).FirstOrDefault () as ReceiveAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();



			Dictionary<string, ICord> cords = new Dictionary<string, ICord> ();

			foreach (var r in ReceiveCords) {

				var inputParameter = r.method.GetParameters ().Select (p => p.ParameterType).FirstOrDefault ();
				var returnType = r.method.ReturnType;
		
				if (inputParameter != null) {
					var spt = typeof(SayingProtocord<>).MakeGenericType (inputParameter);

					var pc =  Activator.CreateInstance(spt, new object[]{r.attr.CordName}) as ISayingCord;

					pc.OnReceive += (arg1, msg) => r.method.Invoke (cordContract, new object[]{ msg });
					cords.Add (pc.Name, pc);
				}
			}

			var AnswerCords = type
				.GetMethods ()
				.Select (m => new 
					{
						method = m, 
						attr = m.GetCustomAttributes (typeof(AnswerAttribute), true).FirstOrDefault () as AnswerAttribute 
					})
				.Where (m => m.attr != null)
				.ToArray ();

			foreach (var r in AnswerCords) {
				var inputParameter = r.method.GetParameters ().Select (p => p.ParameterType).FirstOrDefault ();
				var returnType = r.method.ReturnType;

				if (inputParameter != null && returnType != null) {
					var pc = new AskingProtocord<object, object> (r.attr.QuestionCordName, r.attr.AnswerCordName);
					pc.OnAsking += (ICord arg1, object msg) => r.method.Invoke (cordContract, new object[]{ msg });
					cords.Add (pc.Name, pc);
				}
			}

			var SendCords = type
				.GetProperties ()
				.Select (p => new 
					{
						property = p, 
						attr = p.GetCustomAttributes (typeof(SendAttribute), true).FirstOrDefault () as SendAttribute
					})
				.Where (p => p.attr != null)
				.ToArray ();

			foreach (var sc in SendCords) {
				var del = sc.property.PropertyType;
				var invk = del.GetMethod ("Invoke");
				var prms = invk.GetParameters ();
				if (prms.Length == 1) {
					SayingProtocord<object> cord = null;
					if (!cords.ContainsKey (sc.attr.CordName)) {
						cord = new SayingProtocord<object> (sc.attr.CordName);
					} else
						cord = cords [sc.attr.CordName] as SayingProtocord<object>;

					Action<object> doit = (o) => cord.Send (o);
					sc.property.SetValue (cordContract, doit, null);
					cords.Add (cord.Name, cord);
				}
			}

				var AskCords = type
					.GetProperties ()
					.Select (p => new 
						{
							property = p, 
							attr = p.GetCustomAttributes (typeof(AskAttribute), true).FirstOrDefault () as AskAttribute
						})
					.Where (p => p.attr != null)
					.ToArray ();

				foreach (var ac in AskCords) {
					var adel = ac.property.PropertyType;
					var ainvk = adel.GetMethod ("Invoke");
					var aprms = ainvk.GetParameters ();
					if (aprms.Length == 1 && ainvk.ReturnType != typeof(void)) {

						AskingProtocord<object, object> cord = null;

						if (!cords.ContainsKey (ac.attr.QuestionCordName)) {
							cord = new AskingProtocord<object, object> (ac.attr.QuestionCordName, ac.attr.QuestionCordName);
						} else
							cord = cords [ac.attr.QuestionCordName] as AskingProtocord<object, object>;

						var rt = ainvk.ReturnType;
						var fTT = CreateFuncTTDelegate (aprms [0].ParameterType, rt,(o)=>cord.Ask (o, 10000));
						ac.property.SetValue (cordContract, fTT, null);
						cords.Add (cord.Name, cord);
					}
				}
				
				foreach (var c in cords) {
					Dispatcher.AddCord (c.Value);
				}
			}

		static Delegate CreateFuncTTDelegate(Type inType, Type outType, Func<object, object> fnc)
		{
			var t = typeof(Communicator);
			var mi = t.GetMethod ("ConvertFuncTT", BindingFlags.Static| BindingFlags.NonPublic );
			var miTT =mi.MakeGenericMethod (inType, outType);
			return (Delegate)miTT.Invoke (null, new object[]{ fnc });
		}

		static Func<Tin, Tout> ConvertFuncTT<Tin, Tout>( Func<object, object> fnc)
		{
			Func<Tin, Tout> ans	 = (o) => (Tout)fnc (o);
			return ans;
		}
	}
}
